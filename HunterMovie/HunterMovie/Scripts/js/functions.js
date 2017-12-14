function handleAjax(url,method,dataType,data,success,beforesend,error){if(url!=''){if(typeof(method)=='undefined'){method='POST'}
if(typeof(beforesend)=='undefined'){beforesend=function(){};}
if(typeof(error)=='undefined'){error=function(){};}
if(typeof(success)=='undefined'){success=function(){};}
$.ajax({url:url,type:method,dataType:dataType,data:data,beforeSend:beforesend,success:success,error:error,});}}
function formatNumber(s){if(s==''){return s;}
var n=parseInt(s.replace(/\D/g,''),10);var format=n.toLocaleString();return format;}
function NewCaptcha(elm){new_captcha='/captcha?'+ Math.random();$(elm).attr('src',new_captcha);}
function IsEmail(email){var regex=/^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;return regex.test(email);}
function calc_height_item(){var first_height_item=$(".item.large").height();var height_small_item=(first_height_item- 10)/ 2;$('.list-film .small a').each(function(i,obj){$(this).height(height_small_item+'px');});$(".list-film .large a").each(function(){$(this).height(first_height_item+'px');})}
function calc_height_item_hot(){$("#film-hot .item").each(function(){var $this=$(this);var $a=$this.find('a');var width=$a.width();$a.height(width*0.5625+'px');});}
function readURL(input){if(input.files&&input.files[0]){var reader=new FileReader();reader.onload=function(e){$('#preview-upload').attr('src',e.target.result);$("#preview-upload").cropper({aspectRatio:1/1,autoCropArea:1,crop:function(e){$("#x").val(e.x);$("#y").val(e.y);$("#w").val(e.width);$("#h").val(e.height);}}).cropper('replace',e.target.result);}
reader.readAsDataURL(input.files[0]);}}
function getMobileOperatingSystem(){var userAgent=navigator.userAgent||navigator.vendor||window.opera;if(/windows phone/i.test(userAgent)){return"Windows Phone";}
if(/android/i.test(userAgent)){return"Android";}
if(/iPad|iPhone|iPod/.test(userAgent)&&!window.MSStream){return"iOS";}
return"unknown";}
function createIframe(containerDomObj,id,bodyContent,headContent,width,height){if(typeof containerDomObj!="object")
return false;if(typeof id!="string")
return false;if(typeof bodyContent!="string")
var bodyContent='';if(typeof headContent!="string")
var headContent='';if(typeof width!="string")
var width='100%';if(typeof height!="string")
var height='100%';try{var iframeObj=window.document.createElement('iframe');if(typeof iframeObj!="object"||!iframeObj)
{console.error('Can not create iframe object: '+err.message);return false;}
var html='<!DOCTYPE html><html><head>'+headContent+'</head><body style="margin:0;padding:0">'+bodyContent+'</body></html>';jQuery(containerDomObj).html('');jQuery(containerDomObj).append(iframeObj);jQuery(iframeObj).attr('style','border:0;padding:0;margin:0;width:'+width+';height:'+height);jQuery(iframeObj).attr('scrolling','no');jQuery(iframeObj).attr('id',id);iframeObj.contentWindow.document.open();iframeObj.contentWindow.document.write(html);iframeObj.contentWindow.document.close();if(width=='100%'&&height=='100%')
jQuery(containerDomObj).css('overflow','hidden');return iframeObj;}catch(err){console.error('Create iframe fail: '+err.message);return false;}}
var hook_play_ads=[];var hook_no_play_ads=[];function callNoPlayAds(){$(".ads-under-player").remove();$(".float-ads").remove();$(".ads-balloon-sidebar").remove();for(i=0;i<hook_no_play_ads.length;i++){hook_no_play_ads[i]();}}
function callPlayAds(controller_action){for(i=0;i<hook_play_ads.length;i++){hook_play_ads[i](controller_action);}
jQuery.getJSON("/ajax/getPageAds/"+controller_action,function(data){if(data&&data.length>0){for(i=0;i<data.length;i++){jQuery(data[i]["key"]).html(data[i]["html"]);}}});}
var showAdsDiv=function(){jQuery(".div-ads").show();}
hook_play_ads.push(showAdsDiv);