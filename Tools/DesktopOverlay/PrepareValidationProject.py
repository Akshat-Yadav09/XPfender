"""Prepare an isolated Main Fight Area build from its dependencies and existing scripts."""
import pathlib
import re
import shutil

root = pathlib.Path(__file__).resolve().parents[2]
output = root / 'Temp/DesktopOverlayValidationProject'
output.mkdir(parents=True, exist_ok=True)
guid_map = {}
for meta in (root / 'Assets').rglob('*.meta'):
    match = re.search(r'^guid: (\w+)', meta.read_text(errors='ignore'), re.M)
    if match:
        guid_map[match[1]] = pathlib.Path(str(meta)[:-5])

pending = [root / 'Assets/Scenes/Main Fight Area.unity',
           root / 'Assets/Resources/DesktopOverlay/CanonicalBlockingBubble.prefab']
pending.extend((root / 'Assets').rglob('*.cs'))
pending.extend((root / 'Assets').rglob('*.asmdef'))
pending.extend((root / 'ProjectSettings').glob('*'))
seen = set()
copied_bytes = 0
while pending:
    source = pending.pop()
    if source in seen or not source.is_file():
        continue
    seen.add(source)
    destination = output / source.relative_to(root)
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, destination)
    copied_bytes += source.stat().st_size
    meta = pathlib.Path(str(source) + '.meta')
    if meta.exists():
        pending.append(meta)
    if source.suffix in {'.unity', '.prefab', '.asset', '.mat', '.controller', '.anim',
                         '.overrideController', '.meta', '.renderTexture'}:
        for guid in re.findall(r'guid: ([0-9a-f]{32})', source.read_text(errors='ignore')):
            if guid in guid_map:
                pending.append(guid_map[guid])
    if source.suffix in {'.shader', '.cginc', '.hlsl', '.compute'}:
        for include in re.findall(r'#include\s+["<]([^">]+)[">]', source.read_text(errors='ignore')):
            dependency = source.parent / include
            if dependency.is_file():
                pending.append(dependency)
for name in ['manifest.json', 'packages-lock.json']:
    destination = output / 'Packages' / name
    destination.parent.mkdir(exist_ok=True)
    shutil.copy2(root / 'Packages' / name, destination)
# The validation player only contains Main Fight Area; production Build Settings stay untouched.
(output / 'ProjectSettings/EditorBuildSettings.asset').write_text('''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1045 &1
EditorBuildSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Scenes:
  - enabled: 1
    path: Assets/Scenes/Main Fight Area.unity
    guid: 1a10edd5bd17f7a43af3aa7013d676b2
  m_configObjects: {}
''')
print('Isolated Main Fight Area validation project:', output)
print('Copied dependency/script files:', len(seen), 'bytes:', copied_bytes)
