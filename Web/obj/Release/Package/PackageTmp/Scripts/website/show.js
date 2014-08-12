$.ready(function () {

    var provinceData,cityData={},regionData={},buildingData={};

    $.post("/Area/GetProvince/",function (res) {
        if(res.success&&res.data) {
            provinceData=res.data;
            $.each(provinceData,function (item) {
                $("cmbProvince").options.add(new Option(item.ProvinceName,item.ProvinceID));
            });
        }
    },null,false);

    function loadCity(provinceId,cityId,regionId) {
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
                    loadRegion(cityId,regionId);
                },{
                    provinceId: provinceId
                });
            } else {
                $.each(cityData[provinceId],function (item) {
                    $("cmbCity").options.add(new Option(item.CityName,item.CityID));
                });
                $("cmbCity").value=cityId||0;
                loadRegion(cityId,regionId);
            }
        }
        else
            loadRegion(cityId,regionId);
    };

    function loadRegion(cityId,regionId) {
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
                },{
                    cityId: cityId
                });
            } else {
                $.each(regionData[cityId],function (item) {
                    $("cmbRegion").options.add(new Option(item.RegionName,item.RegionID));
                });
                $("cmbRegion").value=regionId||0;
            }
        }

        $("cmbRegion").on("change");
    };

    $("cmbProvince").on("change",function () {
        loadCity(this.value);
    });

    $("cmbCity").on("change",function () {
        loadRegion(this.value);
    });

    $("cmbRegion").on("change",function () {
        $("cmbBuilding").options.length=1;

        var v=this.value;
        if(v!=0) {
            if(!buildingData[v]) {

                $.post("/Package/GetBuildingsByRegionID/",function (res) {
                    if(res.success&&res.data) {
                        buildingData[v]=res.data;
                        $.each(res.data,function (item) {
                            $("cmbBuilding").options.add(new Option(item.BuildingName,item.BuildingID));
                        });
                    }
                },{
                    regionID: v
                });

            } else {
                $.each(buildingData[v],function (item) {
                    $("cmbBuilding").options.add(new Option(item.BuildingName,item.BuildingID));
                });
            }
        }
    });

    function search(page,pageSize) {
        $("list").html("正在载入...");
        $.post("/Package/GetPackages/",function (res) {
            if(res.success) {
                $("list").html("");
                if(res.total==0)
                    $("list").html("暂无数据。");
                else {
                    var s="";
                    $.each(res.data,function (item) {
                        var memo=item.Memo?item.Memo.replace(/\r\n/g,"<br>").replace(/\r/g,"<br>").replace(/\n/g,"<br>"):"";
                        s+="<dl><dt><a href=\"/Package/"+item.PackageID+".html\">"+item.PackageName+"</a></dt><dd><a href=\"/Package/"+item.PackageID+".html\"><img src=\""+media+item.Url+"\" /></a><p>"+memo+"</p></dd></dl>";

                    });
                    $("list").html(s);
                }
                $.page(page,pageSize,res.total,$("page"),function (pageIndex) {
                    search(pageIndex,pageSize);
                });

            } else {
                $("list").html(res.msg);
            }
        },{
            categoryID: categoryID,
            page: page,
            pageSize: pageSize,
            buildingID: $("cmbBuilding").value,
            regionID: $("cmbRegion").value,
            cityID: $("cmbCity").value,
            provinceID: $("cmbProvince").value,
            keywords: ""
        });
    };

    $("btnSearch").on("click",function () {
        search(1,8);
    })

});