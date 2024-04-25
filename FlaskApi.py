from flask import Flask, request, jsonify
import requests, asyncio, base64, uuid, time, threading, os, shutil, base64, io
import pypyodbc as odbc
import schedule as scheduler
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad, unpad
from datetime import datetime, timedelta
from PIL import Image

sd_link = 'http://127.0.0.1:7861/sdapi/v1/txt2img'
db_cleanup_period = 3
base_storage_path = r'C:\Users\Tomas\Desktop\Aimidge\Storage'

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
    INSERT INTO Users (FirstName, Username, Email, PasswordHash, RegistrationDate, 
    HasUploadedFiles, FileFolderName, UserGuid, TokenExpiration)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
"""

app = Flask(__name__)

@app.route("/stable_diffusion", methods=["POST"])
async def stable_diffusion():
    json_content = request.get_json()
    uid = json_content['uid']
    cursor = sql_connection.cursor()
    cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
    user = cursor.fetchone()
    cursor.close();
    file_Folder = user[6]

    if not get_quota(uid):
        return jsonify({"error": "Quota exceeded!"}), 403

    increaseCount = increase_images_count(uid)
    if increaseCount:
        response = await asyncio.to_thread(requests.post, sd_link, json=json_content)
        try:
            image_data_list = response.json().get('images', [])
            if image_data_list:
                save_directory = os.path.join(base_storage_path, file_Folder, "temp")
                for image_data_base64 in image_data_list:
                    try:
                        image_data_binary = base64.b64decode(image_data_base64)
                        image_filename = os.path.join(save_directory, f"img.jpg")
                        with open(image_filename, 'wb') as image_file:
                            image_file.write(image_data_binary)
                    except Exception as ex:
                        return jsonify((f"Error occured while saving the image!"))
                return jsonify(response.json())
            else:
                return jsonify({"error": "No image data found"}), 500
        except Exception as ex:
            return jsonify({"error": f"Error occured while saving the image!"}), 500
    else:
        return jsonify({"error": "User not found!"}), 404

@app.route("/sd_progress", methods = ["GET"])
async def sd_progress():
    result = requests.get(f"http://127.0.0.1:7861/sdapi/v1/progress?skip_current_image=true")
    percentage = result.json()['progress'] * 100
    return jsonify(percentage)

@app.route("/save_img", methods = ["POST"])
async def save_img():
    json_content = request.get_json()
    uid = json_content['uid']
    cursor = sql_connection.cursor()
    cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
    user = cursor.fetchone()
    cursor.close();
    folder_name = user[6]
    
    source = os.path.join(base_storage_path, folder_name, "temp")
    destination = os.path.join(base_storage_path, folder_name, "gallery")

    source_file = os.path.join(source, "img.jpg")

    file_count = len(os.listdir(destination))
    destination_file = os.path.join(destination, f"img{file_count + 1}.jpg")

    try:
        shutil.move(source_file, destination_file)
        return "", 200
    except Exception as ex:
        return str(ex), 500
    
@app.route("/get_gallery", methods = ["POST"])
async def get_gallery():
    json_content = request.get_json()
    uid = json_content['uid']
    index = json_content.get('index', 1)
    cursor = sql_connection.cursor()
    cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
    user = cursor.fetchone()
    folder_name = user[6]
    cursor.close()

    file_folder = os.path.join(base_storage_path, folder_name, "gallery")

    try:
        img_name = f"img{index}.jpg"
        file_path = os.path.join(file_folder, img_name)
        if (os.path.exists(file_path)):
            with open(file_path, "rb") as file:
                image_data = file.read()
                encoded_image = base64.b64encode(image_data).decode("utf-8")
                return jsonify(encoded_image), 200
    except Exception as ex:
        return str(ex), 500
    
@app.route("/get_gallery_count", methods = ["POST"])
async def get_gallery_count():
    json_content = request.get_json()
    uid = json_content['uid']
    if (uid): 
        cursor = sql_connection.cursor()
        cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
        user = cursor.fetchone()
        folder_name = user[6]
        cursor.close() 
        file_folder = os.path.join(base_storage_path, folder_name, "gallery")
        try:
            file_count = len(os.listdir(file_folder))
            return jsonify({'count': file_count}), 200
        except Exception as ex:
            return str(ex), 500
    else:
        return jsonify({'count': 0}), 200

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
        uid = json_content['UserGuid']
        user_folder = decrypt_user_data(json_content['UserGuid'])
        cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
        existing_user = cursor.fetchone()
        if(existing_user):
            if(json_content['HasUploadedFiles'] == True):
                cursor.execute("UPDATE Users SET HasUploadedFiles = ?, TokenExpiration = ? WHERE UserGuid = ?", (json_content['HasUploadedFiles'], json_content['TokenExpiration'], uid))
                sql_connection.commit()
            else:
                cursor.execute("UPDATE Users SET TokenExpiration = ? WHERE UserGuid = ?", (json_content['TokenExpiration'], uid))
                sql_connection.commit()
        else:
            cursor.execute(sql_insert_unregistered, (json_content['HasUploadedFiles'], user_folder, json_content['UserGuid'], json_content['TokenExpiration']))
            sql_connection.commit()
            create_folder(user_folder)
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
        first_name = json_content['name']
        last_name = json_content['username']

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

        create_folder(existing_folder_str)

        token_expiration = datetime.now() + timedelta(minutes=20)

        cursor.execute(sql_insert_registration, (first_name, last_name, email, password_hash, registration_date, 0, existing_folder_str, user_guid_str, token_expiration))
        sql_connection.commit()
        cursor.close()
        return user_guid_str, 200
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
        if not existing_user:
            cursor.execute("SELECT * FROM Users WHERE Username = ?", (email,))
            existing_user = cursor.fetchone()

        if(existing_user):
            stored_password_hash = existing_user[3]
            if(stored_password_hash == password_hash):
                existing_folder = db_unique_file_folder(cursor)
                existing_folder_str = str(existing_folder)
                user_guid = encrypt_user_data(existing_folder_str)
                user_guid_str = str(user_guid)
                token_expiration = datetime.now() + timedelta(minutes=20)
                cursor.execute("UPDATE Users SET UserGuid = ?, TokenExpiration = ? WHERE Email = ? OR Username = ?", (user_guid_str, token_expiration, email, email))
                sql_connection.commit()
                cursor.close()
                return user_guid_str, 200
            else:
                return "BadPassword", 401
        return "NotFound", 404
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
                'Name': user[0],
                'Username': user[1],
                'Email': user[2],
                'PasswordHash': "*",
                'RegistrationDate': user[4],
                'HasUploadedFiles': user[5],
                'FileFolderName': user[6],
                'UserGuid': user[7],
                'TokenExpiration': user[8],
                'GeneratedImagesCount': user[9]
            }
            user_list.append(user_info)
        cursor.close()
        return jsonify({'DB Users': user_list}), 200
    except Exception as ex:
        return jsonify({'error': str(ex)}), 500
    
@app.route("/db_get_user", methods=["POST"])
async def db_get_user():
    try:
        json_content = request.get_json()
        uid = json_content.get('uid')

        if not uid:
            return jsonify({'error': 'UID parameter is missing'}), 400
        
        cursor = sql_connection.cursor()
        cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
        user = cursor.fetchone()
        cursor.close()
        
        if not user:
            return jsonify({'error': 'User not found'}), 404
        
        user_info = {
            'Name': user[0],
            'Username': user[1],
            'Email': user[2]
        }
        return jsonify(user_info), 200
    except Exception as ex:
        return jsonify({'error': str(ex)}), 500
    
@app.route("/db_update_user", methods=["POST"])
async def db_update_user():
    try:
        json_content = request.get_json()
        username = json_content["username"]
        name = json_content["name"]
        email = json_content["email"]
        uid = json_content.get('uid')

        if not uid:
            return jsonify({'error': 'UID parameter is missing'}), 400
        
        cursor = sql_connection.cursor()
        cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
        user = cursor.fetchone()
        
        print(user)
        if not user:
            return jsonify({'error': 'User not found'}), 404
        
        cursor.execute("UPDATE Users SET Username = ?, FirstName = ?, Email = ? WHERE UserGuid = ?", (username, name, email, uid,))
        sql_connection.commit()
        cursor.close()

        return jsonify("ok"), 200
    except Exception as ex:
        print(str(ex))
        sql_connection.rollback()
        return jsonify({'error': str(ex)}), 500

def get_quota(uid):
    cursor = sql_connection.cursor()
    cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
    user = cursor.fetchone()
    cursor.close() 
    if user:
        if user[0] is not None:
            return True
        else:
            if user[9] < 3:
                return True
        return False
    else:
        return False

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

def create_folder(FileFolderName):
    folder_path = os.path.join(base_storage_path, FileFolderName)
    temp_path = os.path.join(folder_path, "temp")
    gallery_path = os.path.join(folder_path, "gallery")
    try:
        os.makedirs(folder_path)
        os.makedirs(temp_path)
        os.makedirs(gallery_path)
        return True
    except Exception as ex:
        return False
    
def remove_folder(FileFolderName):
    folder_path = os.path.join(base_storage_path, FileFolderName)
    try:
        shutil.rmtree(folder_path)
        return True
    except Exception as ex:
        return False;

async def save_temp_img(img_data, path):
    binary_data = base64.b64decode(img_data)
    stream = io.BytesIO(binary_data)
    img = Image.open(stream)
    img = img.convert("RGBA")
    img.save(path, format="PNG")

def db_user_unique_uid(cursor):
    folder_name = str(db_unique_file_folder(cursor))
    return encrypt_user_data(folder_name)

def db_unique_file_folder(cursor):
    while True:
        FileFolderName = uuid.uuid4()
        cursor.execute("SELECT * FROM Users WHERE FileFolderName = ?", (str(FileFolderName),))
        existing_folder = cursor.fetchone()
        if not existing_folder:
            return FileFolderName
        
def increase_images_count(uid):
    cursor = sql_connection.cursor()
    cursor.execute("SELECT * FROM Users WHERE UserGuid = ?", (uid,))
    user = cursor.fetchone()
    if user:
        generated_images_count = user[9] + 1
        cursor.execute("UPDATE Users SET GeneratedImagesCount = ? WHERE UserGuid = ?", (generated_images_count, uid))
        sql_connection.commit()
        cursor.close()
        return True
    else:
        cursor.close()
        return False
        
def db_cleanup():
    cursor = sql_connection.cursor()
    cursor.execute("SELECT * FROM Users WHERE TokenExpiration < GETDATE() and RegistrationDate IS NULL")
    expired_users = cursor.fetchall()

    for expired_user in expired_users:
        folder_name = expired_user[6]
        remove_folder(folder_name)
        cursor.execute("DELETE From Users WHERE FileFolderName = ?", (folder_name,))
        sql_connection.commit()

def run_flask():
    app.run(debug=True, host='0.0.0.0', port=5000, use_reloader=False)

if __name__ == "__main__":
    flask_thread = threading.Thread(target=run_flask)
    flask_thread.start()

    scheduler.every(db_cleanup_period).minutes.do(db_cleanup)

    while True:
        scheduler.run_pending()
        time.sleep(60)