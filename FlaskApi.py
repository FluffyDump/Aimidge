from flask import Flask, request, jsonify
import requests, asyncio, base64
import pypyodbc as odbc
from Crypto.Cipher import AES

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


app = Flask(__name__)
@app.route("/stable_diffusion", methods=["POST"])
async def stable_diffusion():
    json_content = request.get_json()
    response = await asyncio.to_thread(requests.post, sd_link, json=json_content)
    return jsonify(response.json())

@app.route("/db_post_unregistered", methods = ["POST"])
async def sql_add_unregistered_user():
    json_content =  request.get_json()
    cursor = sql_connection.cursor()
    try:
        FileFolderName = decrypt_user_data(json_content['UserGuid'])
        cursor.execute("SELECT * FROM Users WHERE FileFolderName = ?", (FileFolderName,))
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

def decrypt_user_data(user_data):
    key = b'O1XFeDPaQFAykYcxZZeIM76y1bnTbk92'
    iv = b'6dwejNHPVlRIWXTE'
    cipher = AES.new(key, AES.MODE_CBC, iv)

    user_data_bytes = base64.b64decode(user_data)
    decrypted_user_data = cipher.decrypt(user_data_bytes)

    pad_length = decrypted_user_data[-1]
    decrypted_user_data = decrypted_user_data[:-pad_length]

    decrypted_user_data_str = decrypted_user_data.decode('utf-8')
    return decrypted_user_data_str

if __name__ == "__main__":
    app.run(debug=True, host='0.0.0.0', port=5000)