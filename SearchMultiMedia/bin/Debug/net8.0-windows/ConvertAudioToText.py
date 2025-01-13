import speech_recognition as sr
import os
import sys

sys.stdout.reconfigure(encoding='utf-8')

def recognize_speech_from_audio_in_memory(fileAudio):
    try:
        if not os.path.isfile(fileAudio):
            raise FileNotFoundError(f"File {fileAudio} not found.")

        recognizer = sr.Recognizer()

        with sr.AudioFile(fileAudio) as source:
            audio = recognizer.record(source)

        try:
            text = recognizer.recognize_google(audio, language="vi-VN")
            print(text)
            return text  # Trả về kết quả cho chương trình khác
        except sr.UnknownValueError:
            print("[Google] Không thể nhận diện giọng nói.")
        except sr.RequestError as e:
            print(f"[Google] Lỗi kết nối với dịch vụ nhận diện giọng nói: {e}")
    except Exception as e:
        print(f"Lỗi trong recognize_speech_from_audio_in_memory: {e}")

if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("Usage: python recognize_speech.py <audio_file_path>")
    else:
        recognize_speech_from_audio_in_memory(sys.argv[1])
