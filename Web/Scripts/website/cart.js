$.ready(function () {

    function formatPrice(price) {
        price=price+"";
        return "￥"+price+(/^\d+$/.test(price)?".00":/^\d+\.\d{1}$/.test(price)?"0":"")
    }

    var cart={
        data: null,
        clearList: function () {
            var children=$("productList").childNodes;
            for(var i=children.length-1;i>=0;i--) {
                $(children[i]).remove();
            }
        },
        load: function () {
            this.data=null;
            this.clearList();
            var msg=$("TD",$("TR",$("productList"))).prop("colSpan",9).html("正在载入，请稍候...");
            $.post("/Order/GetCartProductList/",function (res) {
                if(res.success) {
                    if(!res.data||!res.data.length)
                        msg.html("您的购物车中没有商品，<a href=\"/index.html\">随便看看</a>！");
                    else {
                        cart.data=res.data;
                        cart.express={
                            0: "物流"
                        };
                        $.each(res.express,function (item) {
                            cart.express[item.ExpressID]=item.ExpressName;
                        });

                        cart.clearList();
                        var totalAmount=0,total=0,totalPoints=0,totalFreight=0;
                        $.each(res.data,function (data) {
                            row=$("TR",$("productList"));
                            data.checkbox=$("CHECKBOX",$("TD",row).css({ padding: "10px 0px 0px 10px" })).prop("checked",true);
                            $("IMG",$("A",$("TD",row).css({ padding: "10px 0px 0px 0px" })).prop("href",{ "true": "/Product/"+data.ProductID+".html","false": "/Package/"+data.PackageID+".html"}[data.CartType==0||data.CartType==2])).prop("src",media+data.Url);

                            $("A",$("TD",row)).prop("href",{ "true": "/Product/"+data.ProductID+".html","false": "/Package/"+data.PackageID+".html"}[data.CartType==0||data.CartType==2]).html({ "true": data.Name,"false": data.PackageName}[data.CartType==0||data.CartType==2]);
                            $("TD",row).html(data.Code);
                            var price;
                            if(data.CartType==1)
                                price="[套餐]";
                            else if(data.SpecialPrice!=0&&data.SpecialPrice<data.Price)
                                price="<del>"+formatPrice(data.Price)+"</del><br><span style='color:#ac0000'>"+formatPrice(data.SpecialPrice)+"</span>";
                            else
                                price=formatPrice(data.Price);

                            $("TD",row).html(price);
                            var points=$("TD",row).html({ "true": data.BuyQty*data.Points,"false": "-"}[data.CartType==0||data.CartType==2]);
                            var tdQty=$("TD",row);

                            var freight,iAmount,sAmount;

                            if(data.CartType==2||data.CartType==0) {
                                freight=data.Freight+data.Freight1*(data.BuyQty-1);
                                totalFreight+=freight;
                                iAmount=data.BuyQty*(data.SpecialPrice==0?data.Price:data.SpecialPrice);
                                totalAmount+=iAmount;
                                sAmount=iAmount+freight+"";
                            }
                            if(data.CartType==0||data.CartType==1) {

                                $("A",tdQty).css("bagClose").prop("href","javascript:void(0);").on("click",function () {
                                    var qty=parseInt(buyQty.value)
                                    if(qty>1) {
                                        buyQty.value=qty-1;
                                        buyQty.on("change");
                                    }
                                });

                                total+=data.BuyQty;
                                totalPoints+=data.BuyQty*data.Points;

                                var buyQty=$("INPUT",tdQty).css("buyQty")
                                .prop("qty",data.BuyQty)
                                .prop("value",data.BuyQty).on("change",function () {
                                    if(!/^\d+$/.test(this.value)||this.value==0) {
                                        alert("数量必须为大于0的数字");
                                        this.value=this.qty;
                                    }
                                    else {
                                        var oldPoints=this.qty*data.Points;
                                        var oldAmount=this.qty*(data.SpecialPrice==0?data.Price:data.SpecialPrice);
                                        var oldFreight=data.Freight+data.Freight1*(parseInt(this.qty)-1);
                                        var newQty=parseInt(this.value);
                                        var changedQty=newQty-this.qty;

                                        $.post("/Order/AddToCart/",function (res) {
                                            if(res.success) {
                                                if(data.CartType==1) {
                                                    cart.load();

                                                } else {
                                                    total+=changedQty;
                                                    buyQty.qty=newQty;
                                                    data.BuyQty=newQty;
                                                    var newPoints=buyQty.qty*data.Points;
                                                    points.html(newPoints);
                                                    iAmount=buyQty.qty*(data.SpecialPrice==0?data.Price:data.SpecialPrice);
                                                    var newFreight=data.Freight+data.Freight1*(parseInt(newQty)-1);
                                                    sAmount=iAmount+newFreight+"";
                                                    amount.html("￥"+sAmount+(/^\d+$/.test(""+sAmount)?".00":/^\d+\.\d{1}$/.test(""+sAmount)?"0":""));
                                                    //prdFreight.html(newFreight);
                                                    totalAmount+=iAmount-oldAmount;
                                                    totalPoints+=newPoints-oldPoints;
                                                    totalFreight+=newFreight-oldFreight;
                                                    $("lblTotalQty").html(total+"件");
                                                    $("lblTotalAmount").html("￥"+totalAmount);
                                                    $("lblTotalPoints").html(totalPoints+"分");
                                                    //$("lblTotalFreight").html("￥"+totalFreight);
                                                }

                                            }
                                            else {
                                                buyQty.value=buyQty.qty;
                                                alert(res.msg);
                                            }
                                        },{
                                            productId: data.CartType==1?data.PackageID:data.ProductID,
                                            quantity: changedQty,
                                            express: data.Express,
                                            color: data.Color||0,
                                            type: data.CartType==1?1:0,
                                            id: data.CartID
                                        });
                                    }
                                });
                                data.txtBuyQty=buyQty;
                                $("A",tdQty).css("bagOpen").prop("href","javascript:void(0);").on("click",function () {
                                    buyQty.value=parseInt(buyQty.value)+1;
                                    buyQty.on("change");
                                });
                                //$("TD",row).html(cart.express[data.Express]);
                            } else
                                tdQty.html(data.BuyQty);

                            //var prdFreight=$("TD",row).html(data.CartType==2||data.CartType==0?("￥"+freight):"-");
                            $("TD",row).html("-");
                            var amount=$("TD",row).html(data.CartType==2||data.CartType==0?("￥"+sAmount+(/^\d+$/.test(""+sAmount)?".00":/^\d+\.\d{1}$/.test(""+sAmount)?"0":"")):"-");
                            var ctrl=$("TD",row).css({ lineHeight: "20px" });
                            $("A",ctrl).prop("href","javascript:void(0);").html("收藏").on("click",function () {
                                cart.isLogin("/User/AddFav/",function () {
                                    alert("该商品已加入您的收藏！");
                                },{
                                    ids: data.CartType==1?data.PackageID:data.ProductID,
                                    type: data.CartType==1?1:0
                                });
                            });
                            if(data.CartType==1||data.CartType==0) {
                                $("BR",ctrl);
                                $("A",ctrl).prop("href","javascript:void(0);").html("删除").on("click",function () {
                                    if(window.confirm("您确定要移除该商品吗？")) {
                                        if(data.CartType==1) {
                                            cart.removeProd(data.PackageID+"|"+data.CartID+"|-1");
                                        } else {
                                            cart.removeProd(data.ProductID+"|"+data.Express+"|"+(data.Color||0));
                                        }
                                    }
                                });
                            }
                        });

                        $("lblTotalQty").html(total+"件");
                        $("lblTotalAmount").html("￥"+totalAmount);
                        //$("lblTotalFreight").html("￥"+totalFreight);
                        $("lblTotalPoints").html(totalPoints+"分");
                    }
                }
                else
                    msg.html(res.msg);
            });
        },
        isLogin: function (url,callback,params) {
            $.post(url,function (res) {
                if(res.success) {
                    if(callback)
                        callback();
                } else if(res.msg=="IS_NOT_LOGIN") {
                    if(!loginDialog)
                        loginDialog=new LoginDialog();

                    var ids=[];
                    $.each(cart.data,function (item) {
                        if(item.checkbox.checked) {
                            ids.push(item.Cookie);
                        }
                    });
                    if(ids.length==0) {
                        alert("请至少选择一个商品进行结算！");
                        return;
                    }
                    loginDialog.show(false,"Order.html?buy="+ids.join(","));
                } else {
                    alert(res.msg);
                }
            },params);
        },
        removeProd: function (ids) {
            $.post("/Order/RemoveFromCart/",function (res) {
                if(res.success) {
                    alert("删除成功！");
                    cart.load();

                } else
                    alert(res.msg)
            },{
                ids: ids
            })
        },
        init: function () {
            this.load();

            $("prev").on("click",function () {
                window.location.href="/product.html";
            });

            $("next").on("click",function () {

                var ids=[];
                $.each(cart.data,function (item) {
                    if(item.checkbox.checked) {
                        ids.push(item.Cookie);
                    }
                });
                if(ids.length==0) {
                    alert("请至少选择一个商品进行结算！");
                    return;
                }
                cart.isLogin("/Order/CanRedirectToOrderPage/",function () {
                    window.location.href="/Order.html?buy="+ids.join(",");
                });
            });

            $("fav").on("click",function () {
                if(cart.data==null) {
                    alert("您的购物车中没有商品！");
                    return;
                }
                var ids=[],packages=[];
                $.each(cart.data,function (item) {
                    if(item.checkbox.checked) {
                        if(item.CartType==1)
                            ids.push(item.PackageID);
                        else
                            ids.push(item.ProductID);
                    }
                });
                if(ids.length==0) {
                    alert("您还未选择商品！");
                    return;
                }
                cart.isLogin("/User/AddFav/",function () {
                    cart.isLogin("/User/AddFav/",function () {
                        alert("您选择的商品已加入您的收藏！");
                    },{
                        ids: packages.join(","),
                        type: 1
                    });
                },{
                    ids: ids.join(","),
                    type: 0
                });
            });

            $("delete").on("click",function () {
                if(cart.data==null) {
                    alert("您的购物车中没有商品！");
                    return;
                }
                var ids=[];
                $.each(cart.data,function (item) {
                    if(item.checkbox.checked) {
                        if(item.CartType==1)
                            ids.push(data.PackageID+"|"+data.CartID+"|-1");
                        else
                            ids.push(item.ProductID+"|"+item.Express+"|"+(item.Color||0));


                    }
                });

                if(ids.length==0) {
                    alert("您还未选择商品！");
                    return;
                }
                if(window.confirm("您确定要移除这些商品吗？")) {
                    cart.removeProd(ids.join(","));
                }
            });

            $("chkAll").on("click",function () {
                $.each(cart.data,function (item) {
                    item.checkbox.checked=$("chkAll").checked;
                })
            });




        }
    };

    cart.init();


});