(function () {
    var mDialog;
    window.openDialog=function (title,width,height,url,callback) {
        if(!mDialog) {
            mDialog=new function () {
                var frg,container=$("DIV",frg=document.createDocumentFragment()).css({ "position": "relative" });
                var content=$("DIV",container);

                var iframe=$("IFRAME",content,"_myDialog")
                .prop("frameBorder","0")
                .prop("allowTransparency",true)
                .css({ "position": "absolute","zIndex": "1000" });

                var timerID;
                var frameWidth;
                var frameHeight;

                if($.ie)
                    iframe.prop("scrolling","yes")
                    .on("load",function () {
                        var doc=iframe.contentWindow.document;
                        var body=doc.compatMode!='BackCompat'?doc.documentElement:doc.body;
                        var curr=true;
                        timerID=window.setInterval(function () {
                            try {
                                if(frameHeight<body.scrollHeight) {
                                    if(!curr) {
                                        content.style.cssText="";
                                        iframe.style.width=frameWidth+"px";
                                        curr=true;
                                    }

                                } else {
                                    if(curr) {
                                        content.style.cssText="overflow:hidden;position:absolute;zIndex:1000;width:"+frameWidth+"px;height:"+frameHeight+"px;";
                                        iframe.style.width=frameWidth+20+"px";
                                        curr=false;
                                    }
                                }
                            } catch(e) {
                                if(timerID) {
                                    window.clearInterval(timerID);
                                    timerID=0;
                                }
                            }

                        },32);

                    });

                var dlg=new $.Dialog({
                    title: title,
                    content: frg,
                    show: function () {
                        iframe.style.visibility="";
                    },
                    beforeClose: function () {
                        iframe.style.visibility="hidden";
                        if(timerID) {
                            window.clearInterval(timerID);
                            timerID=0;
                        }
                    }
                });
                var callbackFuncs=[];

                this.close=function (success) {
                    dlg.close();

                    for(var i=callbackFuncs.length-1;i>=0;i--) {
                        if(success) {
                            callbackFuncs[i]();
                        }
                        callbackFuncs.splice(i,1);
                    }
                };

                this.show=function (pTitle,pWidth,pHeight,pUrl,pCallback) {
                    window.open(pUrl,"_myDialog");

                    var sW=pWidth+"",sH=pHeight+"";
                    var rect=$.getDocRect();
                    if(/\d+%/.test(sW))
                        pWidth=(rect.clientWidth-2)*parseInt(sW.replace("%",""))/100;
                    if(/\d+%/.test(sH))
                        pHeight=(rect.clientHeight-49)*parseInt(sH.replace("%",""))/100;

                    dlg.title(pTitle);
                    dlg.width(pWidth);
                    iframe.style.width=pWidth-20+"px";
                    iframe.style.height=pHeight+"px";
                    container.style.height=pHeight+"px";
                    frameWidth=pWidth-20;
                    frameHeight=pHeight;
                    content.style.cssText="";
                    dlg.show();
                    if(isFunction(pCallback))
                        callbackFuncs.push(pCallback);
                };

            };
        }
        mDialog.show(title,width,height,url,callback);
    };

    window.closeDialog=function (success) {
        mDialog.close(success);
    };

})();

$.ready(function () {

    function fixContentSize() {
        var left=121;
        var top=82;
        var rect=$.getDocRect();

        var width=Math.max(rect.clientWidth-left,10);
        var height=Math.max(rect.clientHeight-top,10);
        $("content").style.cssText="width:"+width+"px;height:"+height+"px;";
    }

    window.on("resize",fixContentSize);
    fixContentSize();

    $.post("/Manage/Privilege/GetUserPrivileges/",function (res) {
        if(res.success) {
            if(!res.data) {
                alert("您没有使用后台的权限");
            } else {
                var guideItems=[];
                var flag=false;
                $.each(res.data,function (item) {
                    if(item.Privileges) {
                        var gitem={
                            text: item.CategoryName,
                            children: []
                        };
                        if(!flag) {
                            flag=true;
                            gitem.selected=true;
                        }
                        $.each(item.Privileges,function (pitem) {
                            gitem.children.push({
                                text: pitem.PrivilegeName,
                                ico: pitem.Ico,
                                href: pitem.Url
                            });
                        })
                        guideItems.push(gitem);
                    }
                });

                var currentSb;
                function changeSb() {
                    if(currentSb!=this) {
                        if(currentSb)
                            currentSb.css("");
                        currentSb=this;
                        currentSb.css("curTab");
                    }
                    window.open(this.attr("href"),"contentIframe");
                }

                function hoverSb() {
                    this.style.color="#cc0000";
                }
                function outSb() {
                    this.style.color="";
                }

                var currentNav;
                function changeNav() {
                    if(currentNav!=this) {
                        if(currentNav) {
                            currentNav.css("");
                            currentNav.prop("sidebar").css({ "display": "none" });
                        }
                        currentNav=this;
                        currentNav.css("current");
                        var sb=currentNav.prop("sidebar").css({ "display": "block" });
                        changeSb.call(sb.firstChild);
                    }
                }

                var nav=$("nav"),sidebar=$("sidebar");
                $.each(guideItems,function (guideItem) {
                    var sb=$("UL",sidebar).css({ "display": "none" });
                    $.each(guideItem.children||[],function (sbItem) {
                        $("SPAN",$("LI",sb).attr("href",sbItem.href).on("click",changeSb).on("mousemove",hoverSb).on("mouseout",outSb)).css(sbItem.ico).html(sbItem.text);
                    });

                    var nv=$("A",nav).prop("sidebar",sb).attr("href","javascript:void(0);").html(guideItem.text).on("click",changeNav);
                    if(guideItem.selected)
                        changeNav.call(nv);
                });

            }

        } else {
            alert(res.msg);
        }
    });

});