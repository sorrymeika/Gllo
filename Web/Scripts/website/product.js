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
        type: 0,
        ids: id
    });
};

function addToCart(a) {
    var expressType=$$("radFreight").value();
    if(expressType==1)
        expressType=$("cmbExpress").value;
    $.post("/Order/AddToCart/",function (res) {
        if(res.success) {
            if(a)
                window.location.href="/Cart.html";
            else
                alert("放入购物车成功！");

        } else
            alert(res.msg);
    },{
        productId: productID,
        quantity: $("buyQty").value,
        express: expressType,
        type: 0,
        color: (colorId||0)
    });
};



$.ready(function () {
    if(!$.ie6)
        $("toTop").style.left=(document.documentElement.clientWidth-950)/2+950+"px";
    $("toTop").on("click",function () {
        window.scrollTo(0,0);
    });

    //图片
    (function () {
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
                    $("cmbExpress").style.display="none";
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
                    $("cmbExpress").style.display="block";
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

        var colorsCon=$("colors");
        if(colorsCon) {
            var clrs=colorsCon.getElementsByTagName("IMG");
            var currClr=0;
            $.each(clrs,function (color,i) {
                $(color).on("click",function () {
                    if(currClr!=i) {
                        clrs[currClr].css("");
                        currClr=i;
                        clrs[currClr].css("curr");
                    }
                    colorId=parseInt(currClr[i].attr("color"));
                });
            });
        }
    })();

    //评论
    (function () {
        var scoreNum=5;
        var stars=$("score").getElementsByTagName("LI");
        $.each(stars,function (item,i) {
            $(item).on("click",function () {
                scoreNum=i+1;
                $.each(stars,function (starj,j) {
                    if(j<=i) {
                        starj.css("s");
                    } else
                        starj.css("");
                });
            });
        });

        var loadCmt=function (page,pageSize) {
            $("comments").html("正在载入用户评论..");
            $.post("/Product/GetComments/",function (res) {
                if(res.success) {
                    if(res.total==0)
                        $("comments").html("暂无评论。");
                    else {
                        $("comments").html("");
                        $.each(res.data,function (data) {
                            var cmtRow=$("DIV",$("comments")).css("margin:10px 10px 0px 10px; padding-bottom:10px;height:100%;overflow:hidden;border-bottom:1px dotted #ddd;line-height:19px;");
                            $("SPAN",cmtRow).html(data.UserName).css("color:#666;");
                            $("SPAN",cmtRow).html(data.CommentTime).css("margin-left:10px;color:#999;");
                            $("SPAN",cmtRow).html("评分:"+data.Score).css("margin-left:10px;color:#999;");
                            $("BR",cmtRow);
                            $("P",cmtRow).html(data.Content.replace("\r\n","<br>").replace("\r","<br>").replace("\n","<br>")).css("padding:4px;");
                            if(data.Re) {
                                $("P",cmtRow).html("<b>客服回复</b>"+data.Re.replace("\r\n","<br>").replace("\r","<br>").replace("\n","<br>")).css("border:1px solid #dedede;color:#666;padding:4px;margin:4px;background:#f1f1f1;");
                            }
                        });
                    }
                    $.page(page,pageSize,res.total,$("page"),function (pageIndex) {
                        load(pageIndex,pageSize);
                    });
                } else
                    $("comments").html(res.msg);
            },{
                id: productID,
                page: page,
                pageSize: pageSize
            });
        };

        loadCmt(1,10);

        $("btnComment").on("click",function () {
            if(!$("txtComment").value) {
                alert("请填写您的评价！");
                return;
            }
            $.post("/Product/AddComment/",function (res) {
                if(res.success) {
                    alert("恭喜，评价成功！");
                    $("txtComment").value="";
                    $("txtCmtCC").value="";
                    loadCmt(1,10);

                } else if(res.msg=="IS_NOT_LOGIN") {
                    login();
                } else if(res.msg=="IS_NOT_BUY") {
                    alert("您没有购买过该商品，无法发表评论！");
                } else if(res.msg=="IS_COMMENT") {
                    alert("您已对该商品发表过评论，不可重复评论！");
                } else {
                    alert(res.msg);
                }
            },{
                productId: productID,
                content: $("txtComment").value,
                checkCode: $("txtCmtCC").value,
                score: scoreNum
            });
        });

    })();

    //咨询留言
    (function () {

        var loadMessages=function (page,pageSize) {
            $("messages").html("正在载入咨询留言..");
            $.post("/Product/GetMessages/",function (res) {
                if(res.success) {
                    if(res.total==0)
                        $("messages").html("暂无留言。");
                    else {
                        $("messages").html("");
                        $.each(res.data,function (data) {
                            var cmtRow=$("DIV",$("messages")).css("margin:10px 10px 0px 10px; padding-bottom:10px;height:100%;overflow:hidden;border-bottom:1px dotted #ddd;line-height:19px;");
                            $("SPAN",cmtRow).html(data.IsAnonymity?"匿名网友":data.UserName).css("color:#666;");
                            $("SPAN",cmtRow).html(data.AddTime).css("margin-left:10px;color:#999;");
                            $("BR",cmtRow);
                            $("P",cmtRow).html("<b>留言内容：</b>"+data.Content.replace("\r\n","<br>").replace("\r","<br>").replace("\n","<br>")).css("padding:4px;");
                            $("P",cmtRow).html("<b>客服回复：</b>"+(data.Re?data.Re.replace("\r\n","<br>").replace("\r","<br>").replace("\n","<br>"):"暂无回复")).css("padding:4px;color:#f60;");
                        });
                    }
                    $.page(page,pageSize,res.total,$("msgPage"),function (pageIndex) {
                        load(pageIndex,pageSize);
                    });
                } else
                    $("messages").html(res.msg);
            },{
                id: productID,
                page: page,
                pageSize: pageSize
            });
        };

        loadMessages(1,10);

        $("btnMessage").on("click",function () {
            if(!$("txtMessage").value) {
                alert("请填写您的留言！");
                return;
            }
            $.post("/Product/AddMessage/",function (res) {
                if(res.success) {
                    alert("恭喜，留言成功！");
                    $("txtMessage").value="";
                    $("txtMsgCC").value="";
                    loadMessages(1,10);

                } else
                    alert(res.msg);

            },{
                productId: productID,
                content: $("txtMessage").value,
                checkCode: $("txtMsgCC").value,
                isAnonymity: $("isAnonymity").checked?1:0
            });
        });

    })();


    (function () {

        var addressList,addressData,isAddressOpen=false,provinceData,cityData={},regionData={};

        $.post("/Area/GetProvince/",function (res) {
            if(res.success&&res.data) {
                provinceData=res.data;
                $.each(provinceData,function (item) {
                    $("cmbProvince").options.add(new Option(item.ProvinceName,item.ProvinceID));
                });
            }
        },null,false);

        function loadCity(provinceId,cityId,regionId,func) {
            for(var i=$("cmbCity").options.length-1;i>0;i--)
                $("cmbCity").options[i]=null;
            $("cmbProvince").value=provinceId;
            if(provinceId!=0) {
                if(!cityData[provinceId]) {
                    $.post("/Area/GetCity/",function (res) {
                        if(res.success&&res.data) {
                            cityData[provinceId]=res.data;
                            $.each(res.data,function (item) {
                                $("cmbCity").options.add(new Option(item.CityName,item.CityID));
                            });
                        }
                        $("cmbCity").value=cityId||0;
                        loadRegion(cityId,regionId,func);
                    },{
                        provinceId: provinceId
                    });
                } else {
                    $.each(cityData[provinceId],function (item) {
                        $("cmbCity").options.add(new Option(item.CityName,item.CityID));
                    });
                    $("cmbCity").value=cityId||0;
                    loadRegion(cityId,regionId,func);
                }
            }
            else
                loadRegion(cityId,regionId,func);
        }

        function loadRegion(cityId,regionId,func) {
            for(var i=$("cmbRegion").options.length-1;i>0;i--)
                $("cmbRegion").options[i]=null;

            if(cityId!=0) {
                if(!regionData[cityId]) {
                    $.post("/Area/GetRegion/",function (res) {
                        if(res.success&&res.data) {
                            regionData[cityId]=res.data;
                            $.each(res.data,function (item) {
                                $("cmbRegion").options.add(new Option(item.RegionName,item.RegionID));
                            });
                        }
                        $("cmbRegion").value=regionId||0;
                        if(func) {
                            func()
                        }
                    },{
                        cityId: cityId
                    });
                } else {
                    $.each(regionData[cityId],function (item) {
                        $("cmbRegion").options.add(new Option(item.RegionName,item.RegionID));
                    });
                    $("cmbRegion").value=regionId||0;
                    if(func) {
                        func()
                    }
                }
            }
        }

        $("cmbProvince").on("change",function () {
            loadCity(this.value);
        });

        $("cmbCity").on("change",function () {
            loadRegion(this.value);
        });

        $("cmbCity").on("change",function () {
            loadRegion(this.value);
        });

        $("cmbCity").on("change",function () {
            loadRegion(this.value);
        })

        var provID,cityID,regionID;
        var area=$.Cookie.get("area");
        if(area&&/^\d+,\d+,\d+$/.test(area)) {
            var aArea=area.split(",");
            provID=aArea[0];
            cityID=aArea[1];
            regionID=aArea[2];
            loadCity(provID,cityID,regionID,function () {
                getFreight();
            });
        }
        $("sendto").on("click",function () {
            loadCity(provID,cityID,regionID);
            $("areas").css("display:block");
            //            var oldClick=document.body.onclick;

            //            document.body.onclick=function (e) {
            //                e=$E(e);
            //                if($("areas")!=e.target&&!$("areas").contains(e.target)) {
            //                    $("areas").css("display:none");
            //                    document.body.onclick=oldClick;
            //                }
            //            };

        });

        $("closeArea").on("click",function () {
            $("areas").css("display:none");
        });

        function getFreight() {
            $.post("/Product/GetProductFreight/",function (res) {
                if(res.success) {

                    $("tdFreight").html("￥"+res.freight);
                    $("tdFreight1").html("￥"+res.freight1);
                    $("trFreight1").css({ display: res.freight==res.freight1?"none":"" });

                    $("sendto").html("["+$("cmbProvince").options[$("cmbProvince").selectedIndex].text+"/"+$("cmbCity").options[$("cmbCity").selectedIndex].text+"/"+$("cmbRegion").options[$("cmbRegion").selectedIndex].text+"]");
                }
            },{
                productID: productID,
                regionID: regionID
            });
        }

        $("acceptArea").on("click",function () {
            provID=$("cmbProvince").value;
            cityID=$("cmbCity").value;
            regionID=$("cmbRegion").value;
            getFreight();

            $.Cookie.set("area",provID+","+cityID+","+regionID,60);

            $("areas").css("display:none");
        });

    })();

});