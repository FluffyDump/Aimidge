from flask import Flask, request, jsonify
import requests, asyncio, base64, uuid
import pypyodbc as odbc
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad, unpad
from datetime import datetime, timedelta

sd_link = 'http://127.0.0.1:7861/sdapi/v1/txt2img'

DRIVER_NAME = 'SQL SERVER'
SERVER_NAME = 'DESKTOP-L0HRE5L\\SQLEXPRESS'
DATABASE_NAME = 'Aimidge'
connection_string = f"""
    DRIVER={DRIVER_NAME};
    SERVER={SERVER_NAME};
    DATABASE={DATABASE_NAME};
    TRUSTED_CONNECTION=yes;
"""

try: 
    sql_connection = odbc.connect(connection_string)
    print("SQL connection successful")
except Exception as ex:
    print("SQL connection exception: {ex}")

sql_insert_unregistered = """
    INSERT INTO Users (HasUploadedFiles, FileFolderName, UserGuid, TokenExpiration)
    VALUES (?, ?, ?, ?)
"""

sql_insert_registration = """
    INSERT INTO Users (FirstName, LastName, Email, PasswordHash, RegistrationDate, 
    HasUploadedFiles, FileFolderName, UserGuid, TokenExpiration)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
"""

app = Flask(__name__)

@app.route("/stable_diffusion", methods = ["POST"])
async def stable_diffusion():
    json_content = request.get_json()
    response = await asyncio.to_thread(requests.post, sd_link, json=json_content)
    return jsonify(response.json())

@app.route("/sd_progress", methods = ["GET"])
async def sd_progress():
    result = requests.get(f"http://127.0.0.1:7861/sdapi/v1/progress?skip_current_image=true")
    percentage = result.json()['progress'] * 100
    return jsonify(percentage)

@app.route("/sd_api_endpoints", methods = ["GET"])
async def sd_api_endpoints():
    response = requests.get(f"http://127.0.0.1:7861/docs#/")
    return response.content

@app.route("/openapi.json", methods = ["GET"])
async def openApi():
    response = requests.get(f"http://127.0.0.1:7861/openapi.json")
    return response.content

@app.route("/db_post_unregistered", methods = ["POST"])
async def sql_add_unregistered_user():
    json_content = request.get_json()
    cursor = sql_connection.cursor()
    try:
        user_guid = decrypt_user_data(json_content['UserGuid'])
        cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (user_guid,))
        existing_user = cursor.fetchone()
        if(existing_user):
            if(existing_user[7] == decrypt_user_data(json_content['UserGuid'])):
                if(json_content['HasUploadedFiles'] == True):
                    cursor.execute("UPDATE Users SET HasUploadedFiles = ?, TokenExpiration = ? WHERE UserGuid = ?", (json_content['HasUploadedFiles'], json_content['TokenExpiration'], json_content['UserGuid']))
                    sql_connection.commit()
                else:
                    cursor.execute("UPDATE Users SET TokenExpiration = ? WHERE UserGuid = ?", (json_content['TokenExpiration'], json_content['UserGuid']))
                    sql_connection.commit()
            else:
                return jsonify({"error": "UserGuid mismatch with FileFolderName!" }), 400
        else:
            cursor.execute(sql_insert_unregistered, (json_content['HasUploadedFiles'], decrypt_user_data(json_content['UserGuid']), json_content['UserGuid'], json_content['TokenExpiration']))
            sql_connection.commit()
    except Exception as ex:
        sql_connection.rollback()
        return jsonify({"error": str(ex)}), 500
    finally:
        cursor.close()
    return jsonify({"message": "User added successfully"}), 200

@app.route("/db_registration", methods = ["POST"])
async def db_registration():
    json_content = request.get_json()
    cursor = sql_connection.cursor()
    try:
        first_name = json_content['firstName']
        last_name = json_content['lastName']

        email = json_content['email']
        cursor.execute("SELECT * FROM Users WHERE Email = ?", (email,))
        existing_email = cursor.fetchone()
        if(existing_email):
            return "UserExists", 200
        
        password_hash = json_content['passwordHash']
        registration_year = datetime.now().year
        registration_month = datetime.now().month
        registration_day = datetime.now().day
        registration_date = f"{registration_year}-{registration_month:02d}-{registration_day:02d}"

        existing_folder = db_unique_file_folder(cursor)
        existing_folder_str = str(existing_folder)
        user_guid = encrypt_user_data(existing_folder_str)
        user_guid_str = str(user_guid)

        token_expiration = datetime.now() + timedelta(minutes=20)

        cursor.execute(sql_insert_registration, (first_name, last_name, email, password_hash, registration_date, 0, existing_folder_str, user_guid_str, token_expiration))
        sql_connection.commit()
        cursor.close()
        print(user_guid_str)
        return jsonify(user_guid_str), 200
    except Exception as ex:
        print(str(ex))
        return jsonify({"error": str(ex)}), 500
    
@app.route("/db_log_in", methods = ["POST"])
async def db_log_in():
    json_content = request.get_json()
    cursor = sql_connection.cursor()
    try:
        email = json_content['email']
        password_hash = json_content['passwordHash']

        cursor.execute("SELECT * FROM Users WHERE Email = ?", (email,))
        existing_user = cursor.fetchone()
        if(existing_user):
            stored_password_hash = existing_user[3]
            if(stored_password_hash == password_hash):
                existing_folder = db_unique_file_folder(cursor)
                existing_folder_str = str(existing_folder)
                user_guid = encrypt_user_data(existing_folder_str)
                user_guid_str = str(user_guid)
                token_expiration = datetime.now() + timedelta(minutes=20)
                cursor.execute("UPDATE Users SET UserGuid = ?, TokenExpiration = ? WHERE Email =?", (user_guid_str, token_expiration, email))
                sql_connection.commit()
                cursor.close()
                return jsonify(user_guid_str), 200
            else:
                return "BadPassword", 401

        return "NotFound", 200
    except Exception as ex:
        print(str(ex))
        return jsonify({"error": str(ex)}), 500

@app.route("/db_get", methods = ["GET"])
async def db_get():
    try:
        cursor = sql_connection.cursor()
        cursor.execute("SELECT * FROM Users")
        users = cursor.fetchall()

        user_list = []
        for user in users:
            user_info = {
                'FirstName': user[0],
                'LastName': user[1],
                'Email': user[2],
                'PasswordHash': user[3],
                'RegistrationDate': user[4],
                'HasUploadedFiles': user[5],
                'FileFolderName': user[6],
                'UserGuid': user[7],
                'TokenExpiration': user[8]
            }
            user_list.append(user_info)

        cursor.close()
        return jsonify({'DB Users': user_list}), 200
    except Exception as ex:
        return jsonify({'error': str(ex)}), 500

def encrypt_user_data(uid):
    KEY = b"O1XFeDPaQFAykYcxZZeIM76y1bnTbk92"
    INIT_VECTOR = b"6dwejNHPVlRIWXTE"

    cipher = AES.new(KEY, AES.MODE_CBC, INIT_VECTOR)
    padded_data = pad(uid.encode('utf-8'), AES.block_size)
    encrypted_bytes = cipher.encrypt(padded_data)
    return base64.b64encode(encrypted_bytes).decode('utf-8')

def decrypt_user_data(user_data):
    key = b'O1XFeDPaQFAykYcxZZeIM76y1bnTbk92'
    iv = b'6dwejNHPVlRIWXTE'
    cipher = AES.new(key, AES.MODE_CBC, iv)

    user_data_bytes = base64.b64decode(user_data)
    decrypted_user_data_bytes = cipher.decrypt(user_data_bytes)

    decrypted_user_data = unpad(decrypted_user_data_bytes, AES.block_size)
    decrypted_user_data_str = decrypted_user_data.decode('utf-8')
    return decrypted_user_data_str

def db_user_uid(cursor):
    folder_name = str(db_unique_file_folder(cursor))
    return encrypt_user_data(folder_name)

def db_unique_file_folder(cursor):
    while True:
        FileFolderName = uuid.uuid4()
        cursor.execute("SELECT * FROM Users WHERE FileFolderName = ?", (str(FileFolderName),))
        existing_folder = cursor.fetchone()
        if not existing_folder:
            return FileFolderName

if __name__ == "__main__":
    app.run(debug=True, host='0.0.0.0', port=5000)