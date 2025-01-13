import librosa
import sys
import os

def cal_mfcc(fileAudio):
    try:
        # Kiểm tra loại file (WAV hoặc MP3)
        if not os.path.isfile(fileAudio):
            raise FileNotFoundError(f"File {fileAudio} not found.")

        # Đọc tệp âm thanh (hỗ trợ cả WAV và MP3)
        y, sr = librosa.load(fileAudio, sr=None)  # sr=None giữ nguyên tần số mẫu của tệp gốc

        mfcc_feat = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=13)

        with open('mfcc.txt', 'w', encoding='utf-8') as myFile:
            t = ''
            for i in range(mfcc_feat.shape[1]):
                for j in range(mfcc_feat.shape[0]):
                    t += str(mfcc_feat[j, i])
                    if j < mfcc_feat.shape[0] - 1:
                        t += ' '
                t += '\n'

            myFile.write(t)

    except Exception as e:
        print(f"Lỗi trong cal_mfcc: {str(e)}".encode('utf-8', errors='ignore').decode('utf-8'))

if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("Usage: python cal_mfcc.py <audio_file_path>")
    else:
        cal_mfcc(sys.argv[1])
