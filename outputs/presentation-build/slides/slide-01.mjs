export async function addSlide(presentation, ctx) {
  const slide = presentation.slides.add();
  ctx.addShape(slide, {x:0,y:0,w:1280,h:720,fill:'#FFF7FE'});
  ctx.addText(slide, {text:'SwipeMate', x:80,y:80,w:800,h:80,fontSize:54,bold:true,color:'#111827'});
  ctx.addText(slide, {text:'Мобилно приложение за колективно вземане на решения', x:80,y:160,w:900,h:50,fontSize:24,color:'#4B5563'});
  return slide;
}
