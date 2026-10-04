"""Extract the canonical scene bubble without writing or connecting the source scene."""
import hashlib
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parents[2]
SCENE = ROOT / 'Assets/Scenes/Main Fight Area.unity'
PREFAB = ROOT / 'Assets/Resources/DesktopOverlay/CanonicalBlockingBubble.prefab'


def extract():
    original = SCENE.read_bytes()
    text = original.decode('utf-8-sig')
    blocks = {key: (header, body) for header, key, body in re.findall(
        r'(--- !u!\d+ &(\d+)[^\n]*\n)(.*?)(?=--- !u!|\Z)', text, re.S)}
    bubble_id = '730795987'
    components = re.findall(r'component: {fileID: (\d+)}', blocks[bubble_id][1])
    assert len(components) == 4
    assert '63e3a994a7786f54db11d536e3dfcb4b' in blocks[components[-1]][1]
    transform_id = components[0]
    scale = [1.0, 1.0, 1.0]
    ancestor = transform_id
    while ancestor != '0':
        body = blocks[ancestor][1]
        values = re.search(r'm_LocalScale: {x: ([^,]+), y: ([^,]+), z: ([^}]+)}', body)
        rotation = re.search(r'm_LocalRotation: {x: ([^,]+), y: ([^,]+), z: ([^,]+), w: ([^}]+)}', body)
        assert all(float(rotation[i]) == 0 for i in [1, 2, 3]), 'Rotated parent: use Unity prefab extraction instead.'
        scale = [a * float(b) for a, b in zip(scale, values.groups())]
        ancestor = re.search(r'm_Father: {fileID: (\d+)}', body).group(1)
    parts = ['%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n']
    for component_id in [bubble_id] + components:
        header, body = blocks[component_id]
        if component_id == transform_id:
            body = re.sub(r'm_Father: {fileID: \d+}', 'm_Father: {fileID: 0}', body)
            body = re.sub(r'm_LocalPosition: .*', 'm_LocalPosition: {x: 0, y: 0, z: 0}', body)
            body = re.sub(r'm_LocalScale: .*', 'm_LocalScale: {x: %.9g, y: %.9g, z: %.9g}' % tuple(scale), body)
        parts.append(header + body)
    PREFAB.parent.mkdir(parents=True, exist_ok=True)
    PREFAB.write_text(''.join(parts), encoding='utf-8', newline='\n')
    assert SCENE.read_bytes() == original, 'Source scene changed during extraction.'
    print('Extracted exact SpriteRenderer, BoxCollider2D and existing bubble script blocks.')
    print('Baked source world scale:', scale)
    print('Source scene unchanged:', hashlib.sha256(original).hexdigest())


if __name__ == '__main__':
    extract()
