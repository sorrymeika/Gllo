$.ready(function () {

    var pics=$("prodSPicList").getElementsByTagName("IMG");
    var bPics=$("prodPics").getElementsByTagName("IMG");
    var vPics=$("viewPics").getElementsByTagName("IMG");
    if(pics&&pics.length>0) {
        var curr=0;
        $.each(pics,function (pic,i) {
            $(pic).on("click",function () {
                if(i!=curr) {
                    bPics[curr].style.display="none";
                    vPics[curr].style.display="none";
                    pics[curr].css("");
                    curr=i;
                    bPics[curr].style.display="";
                    vPics[curr].style.display="";
                    pics[curr].css("curr");
                }

            });

            $(bPics[i]).on("mousemove",function (e) {
                $("viewPics").style.display="block";
                var x,y;
                if(window.event) {
                    x=window.event.offsetX;
                    y=window.event.offsetY;
                } else {
                    x=e.layerX;
                    y=e.layerY;
                }
                var x=x*2-150;
                var y=y*2-150;
                vPics[i].style.cssText="left:"+(x* -1)+"px;top:"+(y* -1)+"px";

            }).on("mouseout",function () {
                $("viewPics").style.display="none";
            });

        });
        if(pics.length>3) {
            var index=0;
            var pic=$("picC");

            function checkBtn() {
                if(index==0)
                    $("prevPic").css("dsb");
                else
                    $("prevPic").css("")

                if(index==pics.length-3)
                    $("nextPic").css("dsb");
                else
                    $("nextPic").css("")
            };

            $("nextPic").css("").on("click",function () {
                if(index!=pics.length-3) {
                    var w=index* -90,end=(index+1)* -90;
                    var timer=window.setInterval(function () {
                        w-=6;

                        if(w<=end) {
                            pic.style.marginLeft=end+"px";
                            window.clearInterval(timer);
                        } else
                            pic.style.marginLeft=w+"px";

                    },20);

                    index+=1;
                }

                checkBtn();

            });

            $("prevPic").css("dsb").on("click",function () {
                if(index!=0) {
                    var w=index* -90,end=(index-1)* -90;
                    var timer=window.setInterval(function () {
                        w+=6;

                        if(w>=end) {
                            pic.style.marginLeft=end+"px";
                            window.clearInterval(timer);
                        } else
                            pic.style.marginLeft=w+"px";

                    },20);

                    index-=1;
                }

                checkBtn();
            });
        }
    }

    var packageData;
    $.post("/Package/GetPackageDetails/",function (res) {
        if(res.success) {
            packageData=res.data;

            if(packageData!=null) {

                var frg=document.createDocumentFragment();
                var container=$("DIV",frg).css("pcont");
                var opened=null,currDC=null,currCont=null;
                $.each(packageData,function (data,i) {
                    var item=$("DIV",$("group")).css("item");
                    var itemCon=$("DIV",item).css("itemCon");
                    $("STRONG",itemCon).html(data.PpcName);
                    var children=data.children;
                    var firstChild=children[0];
                    var img=$("IMG",itemCon).prop("src",media+firstChild.Url);
                    var name=$("P",itemCon).html(firstChild.Name);
                    var cont=container;
                    var detailCon=$("DIV",cont).css("dcon");
                    var change=$("A",item).prop("href","javascript:;").html("替换").on("click",function () {
                        if(opened!==this) {
                            if(opened) {
                                opened.css("");
                                if(currCont!=cont)
                                    currCont.css("height:0px;");
                                currDC.css("display:none;")
                            }
                            currCont=cont;
                            currDC=detailCon;
                            opened=this;
                            this.css("curr");
                            cont.css("height:auto;");
                            detailCon.css("display:block;");
                        } else {
                            cont.css("height:0px;");
                            detailCon.css("display:none;");
                            this.css("");
                            opened=null;
                            currCont=null;
                            currDC=null;
                        }
                    });
                    $.each(children,function (child) {
                        var cItem=$("DIV",detailCon).css("citem");

                        $("IMG",cItem).prop("src",media+child.Url);
                        $("P",cItem).html(firstChild.Name);
                        var select=$("A",cItem).prop("href","javascript:;").html("选择").on("click",function () {
                            img.src=media+child.Url;
                            name.html(child.Name);
                            change.on("click");
                        });
                    });

                    if((i+1)%5==0||i==packageData.length-1) {
                        $("group").appendChild(container);

                        container=$("DIV",frg);
                    }
                });

            }
        }
    },{
        id: packageID
    })


});