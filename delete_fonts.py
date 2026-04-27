import os
import shutil

base = 'Assets/Art/Fonts/나눔 글꼴'

# Keep only NanumSquareNeo and 나눔고딕
keep = ['NanumSquareNeo', 'NanumSquareNeo.meta', '나눔고딕', '나눔고딕.meta']

for item in os.listdir(base):
    if item not in keep:
        path = os.path.join(base, item)
        if os.path.isdir(path):
            shutil.rmtree(path)
            print(f'Removed dir: {item}')
        else:
            os.remove(path)
            print(f'Removed file: {item}')

print('Done')
