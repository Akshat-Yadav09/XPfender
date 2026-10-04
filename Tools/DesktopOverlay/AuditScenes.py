import pathlib
import re

root = pathlib.Path(__file__).resolve().parents[2]
metas = {}
for p in (root / 'Assets').rglob('*.cs.meta'):
    metas[re.search(r'guid: (\w+)', p.read_text()).group(1)] = str(p.relative_to(root))[:-5]
for path in ['Assets/Scenes/Main Fight Area.unity',
             'Assets/Ash ke assests/Extra scenes/BuggedWindows.unity',
             'Assets/Ash ke assests/Extra scenes/Antivirus.unity',
             'Assets/Scenes/Last Scene Wining.unity']:
    text = (root / path).read_text()
    blocks = dict(re.findall(r'--- !u!\d+ &(\d+)[^\n]*\n(.*?)(?=--- !u!|\Z)', text, re.S))
    print(path)
    for i, b in blocks.items():
        if 'm_Name: png-transparent-soap-bubble' in b:
            components = []
            for c in re.findall(r'component: {fileID: (\d+)}', b):
                component = blocks.get(c, '')
                script = re.search(r'm_Script:.*guid: (\w+)', component)
                components.append(metas.get(script.group(1), '?') if script else component.splitlines()[0])
            print('  bubble', i, components)
    print('  Spawners:', text.count('057c9355374e4b64f86d3d215ce464c6'),
          'BigBubbleDestroyOnCollision:', text.count('63e3a994a7786f54db11d536e3dfcb4b'))
