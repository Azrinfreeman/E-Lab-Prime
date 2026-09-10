import asyncio, json, re
from pathlib import Path
import edge_tts
root=Path(__file__).resolve().parents[1]
source=(root/'Assets/Script/3d/Lab4VoiceInstructions.cs').read_text(encoding='utf-8-sig')
lines=re.findall(r'^        "(.+)"[,]?$',source,re.M)
assert len(lines)==3
out=root/'Assets/Audio/Lab4/Instructions'
out.mkdir(parents=True,exist_ok=True)
async def main():
    for i,line in enumerate(lines):
        path=out/f'step_{i+1}_ms.mp3'
        await edge_tts.Communicate(line,'ms-MY-OsmanNeural',rate='-8%').save(str(path))
        print(path.name,path.stat().st_size)
    (out/'narration.json').write_text(json.dumps({'generator':'edge-tts','voice':'ms-MY-OsmanNeural','rate':'-8%','synthetic':True,'lines':lines},ensure_ascii=False,indent=2),encoding='utf-8')
asyncio.run(main())


