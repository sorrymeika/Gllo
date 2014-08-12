function addToFav(id) {
    $.post("/User/AddFav/",function (res) {
        if(res.success) {
            alert("您选择的商品已加入您的收藏！");

        } else if(res.msg=="IS_NOT_LOGIN") {
            login();
        } else {
            alert(res.msg);
        }
    },{
        type: 1,
        ids: id
    });
};

function addToCart(a) {
    var prds=[];
    for(var ppcID in selectedProducts) {
        prds.push(ppcID+"|"+selectedProducts[ppcID].PpID);
    };
    $.post("/Order/AddToCart/",function (res) {
        if(res.success) {
            if(a)
                window.location.href="/Cart.html";
            else
                alert("放入购物车成功！");

        } else
            alert(res.msg);
    },{
        packageID: packageID,
        quantity: 1,
        type: 1,
        prds: prds.join(","),
        id: -1
    });
};
var selectedProducts={};

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
                var amount=0;
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

                    selectedProducts[data.PpcID]=firstChild;
                    amount+=firstChild.SpecialPrice!=0&&firstChild.SpecialPrice<firstChild.Price?firstChild.SpecialPrice:firstChild.Price;
                    $.each(children,function (child) {
                        var cItem=$("DIV",detailCon).css("citem");

                        $("IMG",cItem).prop("src",media+child.Url);
                        $("P",cItem).html(child.Name);
                        var select=$("A",cItem).prop("href","javascript:;").html("选择").on("click",function () {
                            img.src=media+child.Url;
                            name.html(child.Name);
                            change.on("click");
                            amount-=selectedProducts[data.PpcID].SpecialPrice!=0&&selectedProducts[data.PpcID].SpecialPrice<selectedProducts[data.PpcID].Price?selectedProducts[data.PpcID].SpecialPrice:selectedProducts[data.PpcID].Price
                            selectedProducts[data.PpcID]=child;
                            amount+=child.SpecialPrice!=0&&child.SpecialPrice<child.Price?child.SpecialPrice:child.Price;
                            $("price").html("￥"+amount+"/套");
                        });
                    });

                    if((i+1)%5==0||i==packageData.length-1) {
                        $("group").appendChild(container);

                        container=$("DIV",frg);
                    }
                });
                $("price").html("￥"+amount+"/套");

            }
        }
    },{
        id: packageID
    })

    if(!$.ie6)
        $("toTop").style.left=(document.documentElement.clientWidth-950)/2+950+"px";
    $("toTop").on("click",function () {
        window.scrollTo(0,0);
    });

});