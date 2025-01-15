import pandas as pd
import os
import re
import speech_recognition as sr
from pydub import AudioSegment
import librosa
import soundfile as sf
from io import BytesIO

# Hàm xóa dấu câu trong title và description
def clean_text(text):
    return re.sub(r'[.,-:\'\"]', '', text)

# Chuẩn bị và xử lý âm thanh bằng đối tượng BytesIO
def preprocess_audio_in_memory(input_file):
    # Đọc file âm thanh
    print("Đang xử lý âm thanh...")

    # 1. Tăng âm lượng và chuẩn hóa
    sound = AudioSegment.from_file(input_file)
    normalized_sound = sound.normalize()

    # 2. Xuất âm thanh chuẩn hóa ra đối tượng BytesIO
    temp_buffer = BytesIO()
    normalized_sound.export(temp_buffer, format="wav")
    temp_buffer.seek(0)

    # 3. Chuyển đổi tần số mẫu về 16 kHz sử dụng librosa
    y, sr = librosa.load(temp_buffer, sr=16000)

    # 4. Lưu vào BytesIO để trả về
    output_buffer = BytesIO()
    sf.write(output_buffer, y, 16000, format="wav")
    output_buffer.seek(0)
    print("Xử lý âm thanh hoàn tất.")
    return output_buffer

# Nhận diện giọng nói từ âm thanh đã xử lý
def recognize_speech_from_audio_in_memory(file):
    recognizer = sr.Recognizer()

    with sr.AudioFile(file) as source:
        print("Đang lắng nghe..." + file)
        audio = recognizer.record(source)

    try:
        text = recognizer.recognize_google(audio, language="vi-VN")
        print("Nhận diện giọng nói thành công.")
        return text
    except sr.UnknownValueError:
        print("Không thể nhận diện giọng nói.")
        return ""
    except sr.RequestError as e:
        print(f"Lỗi kết nối với dịch vụ nhận diện giọng nói: {e}")
        return ""

# Đọc và xử lý tệp CSV
csv_file = 'audio_update.csv'  # Đường dẫn tệp CSV của bạn
df = pd.read_csv(csv_file)

# Loại bỏ dấu câu trong cột title và description
df['title'] = df['title'].apply(clean_text)
df['description'] = df['description'].apply(clean_text)

# Thêm cột content nếu chưa có
if 'content' not in df.columns:
    df['content'] = ""

check = 1

# Duyệt qua các dòng trong CSV và nhận diện giọng nói từ file vocals.wav
for index, row in df.iterrows():
    file_name = row['fileName']
    folder_path = os.path.join('audio_vocals', file_name.replace('.mp3', '').replace("'", "_"))  # Thư mục con theo tên fileName

    vocals_file = os.path.join(folder_path, 'vocals.wav')  # Đường dẫn đến tệp vocals.wav

    if not os.path.exists(vocals_file):
        print(f"Không tìm thấy tệp: {vocals_file}")

    if os.path.exists(vocals_file):
        print(f"Đang xử lý âm thanh {check}/129 ...")
        
        # processed_audio = preprocess_audio_in_memory(vocals_file)
        # content = recognize_speech_from_audio_in_memory(processed_audio)
        
        content = recognize_speech_from_audio_in_memory(vocals_file)
        
        check += 1
        
        # Cập nhật cột content trong dataframe
        df.at[index, 'content'] = content

# Lưu lại tệp CSV với cột content mới
df.to_csv('updated_audio2.csv', index=False)
