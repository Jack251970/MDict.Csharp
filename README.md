# MDict.Csharp

mdict (\*.mdd \*.mdx) file reader based on [terasum/js-mdict](https://github.com/terasum/js-mdict).

Thanks to [terasum](https://github.com/terasum).

## Usage

```csharp
using MDict.Csharp.Models;

var mdict = new MdxDict("resources/oald7.mdx");

var def = mdict.Lookup("ask");
Console.WriteLine(def.Definition);

/*
<head><link rel="stylesheet" type="text/css" href="O7.css"/></head><body><span class="hw"> ask </span hw><span class="i_g"> <img src="key.gif"/>  /<a class="i_phon" href="sound://aask_ggv_r1_oa013910.spx">ɑ:sk</a i_phon><span class="z">; </span z><i>NAmE</i> <a class="y_phon" href="sound://aask_ggx_r1_wpu01057.spx">æsk</a y_phon>​/ </span i_g><span class="cls"> verb</span cls><br><span class="sd">QUESTION<span class="chn"> 问题</span chn></span sd>
<div class="define"><span class="numb">1</span numb><span class="cf"> ~ <span class="bra">(</span bra>sb<span class="bra">)</span bra> <span class="bra">(</span bra>about sb/ sth<span class="bra">)</span bra> </span cf><span class="d">to say or write sth in the form of a question, in order to get information<span class="chn"> 问；询问</span chn></span d></div define>
<span class="phrase"><span class="pt">  [<span class="pt_inside">V <span class="pt_bold">speech</span></span><span>]</span> </span pt></span phrase>
<span class="sentence_eng">'Where are you going?' she asked. </span sentence_eng>
<span class="sentence_chi">"你去哪里？"她问道。</span sentence_chi>
<span class="phrase"><span class="pt"> [<span class="pt_inside">VN <span class="pt_bold">speech</span></span><span>]</span> </span pt></span phrase>
<span class="sentence_eng">'Are you sure?' he asked her. </span sentence_eng>
...
</body>
*/

var mdx = new MddDict("./tests/data/oale8.mdd");
Console.WriteLine(mdx.Locate("\\Logo.jpg"));

/*
$ git clone github.com/terasum/js-mdict
$ cd js-mdict
$ npx tsx ./example/oale8-mdd-example.ts

NOTE: the mdd's definition is base64 encoded bytes, 
if your target is css/js content, please decode base64 and get the original text
if your target is images, you can use dataurl to show the images

{
  keyText: '\\Logo.jpg',
  definition: '/9j/4AAQSkZJRgABAgAAAQABAAD//gAEKgD/4gIcSUNDX1BST0ZJTEUAAQEAAAIMbGNtcwIQ...'
 }
*/
```
