import pandas as pd
import pyodbc

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

def map_category_to_name(category_url):
    category_mapping = {
        "thoi-su": "Thời sự",
        "the-gioi": "Thế giới",
        "kinh-doanh": "Kinh doanh",
        "giai-tri": "Giải trí",
        "the-thao": "Thể thao",
        "phap-luat": "Pháp luật",
        "giao-duc": "Giáo dục",
        "suc-khoe": "Sức khỏe",
        "doi-song": "Đời sống",
        "du-lich": "Du lịch",
        "khoa-hoc": "Khoa học",
        "so-hoa": "Số hóa",
        "oto-xe-may": "Ô tô - Xe máy",
        "y-kien": "Ý kiến",
        "tam-su": "Tâm sự"
    }
    
    for key, value in category_mapping.items():
        if key in category_url:
            return value
    return "Khác"

def insert_from_csv_to_sql(conn, csv_file, table_name):
    df = pd.read_csv(csv_file, encoding="utf-8")

    df['link'] = df['link'].fillna('')
    df['content'] = df['content'].fillna('')  
    df['title'] = df['title'].fillna('') 
    df['description'] = df['description'].fillna('')  
    df['category'] = df['category'].fillna('')

    df['TheLoai'] = df['category'].apply(map_category_to_name)

    cursor = conn.cursor()
    for index, row in df.iterrows():  
        query = f"""
        INSERT INTO {table_name} (Link, TieuDe, NoiDungTomTat, NoiDung, TheLoai)
        VALUES (?, ?, ?, ?, ?)
        """
        cursor.execute(query, (
            row['link'],
            row['title'],  
            row['description'],
            row['content'],
            row['TheLoai']
        ))
    conn.commit()
    print("Dữ liệu đã được import thành công!")

def main():
    csv_file = "text.csv"  
    table_name = "VanBan"  
    conn = connect_to_sql_server()
    if conn:
        insert_from_csv_to_sql(conn, csv_file, table_name)
        conn.close()

if __name__ == "__main__":
    main()
