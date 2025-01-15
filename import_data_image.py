import pandas as pd
import pyodbc
import re

def connect_to_sql_server():
    try:
        connection_string = (
            "Driver={SQL Server};"  # Driver cho SQL Server
            "Server=LAPTOP-79T4Q5ET\\BACH;"  # Tên server (thay YOUR_SERVER_NAME)
            "Database=DPT;"  # Tên database (thay YOUR_DATABASE_NAME)
            "Trusted_Connection=yes;"  # Sử dụng Windows Authentication
        )
        
        # Thực hiện kết nối
        conn = pyodbc.connect(connection_string)
        print("Kết nối thành công đến SQL Server!")
        return conn
    except Exception as e:
        print(f"Lỗi kết nối: {e}")
        return None

def clean_text(text):
    return re.sub(r'[.,-:\'\"]', '', text)

def insert_from_csv_to_sql(conn, csv_file, table_name):
    df = pd.read_csv(csv_file, encoding="utf-8")

    df['link'] = df['link'].fillna('')
    df['fileName'] = df['fileName'].fillna('')  
    df['title'] = df['title'].fillna('') 
    df['title'] = df['title'].apply(clean_text)
    df['category'] = df['category'].fillna('')

    cursor = conn.cursor()
    for index, row in df.iterrows():  
        query = f"""
        INSERT INTO {table_name} (Link, TenFile, TieuDe, TheLoai)
        VALUES (?, ?, ?, ?)
        """
        cursor.execute(query, (
            row['link'],
            row['fileName'],  
            row['title'],
            row['category']
        ))
    conn.commit()
    print("Dữ liệu đã được import thành công!")

def main():
    csv_file = "image.csv"  
    table_name = "HinhAnh"  
    conn = connect_to_sql_server()
    if conn:
        insert_from_csv_to_sql(conn, csv_file, table_name)
        conn.close()

if __name__ == "__main__":
    main()
