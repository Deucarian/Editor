const fs = require('node:fs');
const path = require('node:path');
const sharp = require('sharp');

async function render() {
  const source = path.join(__dirname, 'svg');
  const target = path.resolve(__dirname, '../../../Editor/Assets/Icons/Lucide');
  for (const name of fs.readdirSync(source).filter(name => name.endsWith('.svg'))) {
    const svg = fs.readFileSync(path.join(source, name), 'utf8').replaceAll('currentColor', '#ffffff');
    await sharp(Buffer.from(svg), { density: 384 }).resize(128, 128).png().toFile(path.join(target, name.replace(/\.svg$/, '.png')));
  }
}
render().catch(error => { console.error(error.message); process.exitCode = 1; });
