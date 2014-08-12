﻿(function () {
    var guid=0;

    $.Grid=function (config) {

        var _self=this;
        var selectedRows=[];
        var data=[];
        var selectedRow=null;
        var rows=[];
        var columns=[];
        var jsonResult=null;
        var baseParams;
        var gridPage;
        var contextMenu=null;
        var menu=null;
        var contextMenuCell=null;
        var configCol=config.columns;
        var reset=function () {
            while(data.length) {
                data.pop();
            }
            while(rows.length!=0) {
                rows.pop();
            }
        };
        var showMsg=function (msg) {
            $("LI",$("UL",$(content).html("")).css("gridRow")).css("gridCell").css({ width: "100%",lineHeight: "26px",textAlign: "left",textIndent: "1em" }).html(msg);
        };
        var load=function (url,params,callback) {
            if(!url&&!params) {
                url=config.url;
                if(typeof baseParams=="undefined")
                    baseParams={};
                callback=config.callback;
            }
            else {
                config.url=url;
                if(callback)
                    config.callback=callback;
                baseParams=params||{};
            }
            if(config.pageEnabled) {
                baseParams.page=baseParams.page||1;
                baseParams.pageSize=baseParams.pageSize||10;
            }
            showMsg("正在载入数据...");
            $.post(url,function (res) {
                var page;

                jsonResult=res;
                if(res.success) {
                    loadData(res.data);
                    if(config.pageEnabled)
                        page=$.page(baseParams.page,baseParams.pageSize,res.total,gridPage,function (pageIndex) {
                            baseParams.page=pageIndex;
                            load();
                        },true);
                } else {
                    reset();
                    showMsg(res.msg);
                }
                if(isFunction(config.callback))
                    config.callback(res,page);
            },baseParams);
        };
        var loadData=function (dat) {
            content.innerHTML="";
            reset();
            if(dat&&dat.length) {
                for(var i=0;i<dat.length;i++) {
                    appendData(dat[i]);
                }
            } else {
                showMsg("暂无数据。");
            }
        };
        var addColumn=function (column) {
            var headerCell;
            var headerText;

            column.columnIndex=columns.length;
            columns.push(column);

            headerCell=$("LI",header).css({ "width": column.width=="auto"?"auto":(column.width+"%") });
            if(column.columnIndex==0&&config.children) {
                headerCell=$("DIV",headerCell).css("gridPlus").on("click",function (e) {
                    if(e.target==this) {
                        if(this.minus) {
                            this.css("gridPlus");
                            $.each(rows,function (row) {
                                row.expand();
                            });

                        } else {
                            this.css("gridMinus");
                            $.each(rows,function (row) {
                                row.shrink();
                            });
                        }
                        this.minus=!this.minus;
                    }
                }).prop("minus",false);
                headerCell.css({ "textAlign": "left" });
            }
            else if(column.align)
                headerCell.css({ "textAlign": column.align });

            column.element=headerCell;

            headerText=$("A",headerCell).html(column.header||"");
            if(column.sortAble) {
                headerText.style.cursor="pointer";
                headerText.onclick=function () {
                    if(config.pageEnabled&&typeof baseParams!=="undefined") {
                        baseParams.sort=column.columnName;

                        this.isAsc=!!this.isAsc;
                        baseParams.isAsc=this.isAsc;
                        this.className=this.isAsc?"":"";

                        load();
                    }
                };
            }
            $("EM",headerText);
        };
        var appendData=function (rowData) {
            appendRow(rowData);
            data.push(rowData);
        };
        var appendRow=function (rowData) {
            var gridRow=$("UL",content)
                .cssText("width:100%;height:"+(config.rowHeight)+"px")
                .css("gridRow")
                .on("click",function (e) {
                    if(!config.multiselect) {
                        if(selectedRow) {
                            if(selectedRow.selector)
                                selectedRow.selector.checked=false;
                            selectedRow.element.className="gridRow";
                        }
                        row.element.className="gridRow gridRowCur";
                        if(row.selector)
                            row.selector.checked=true;

                        selectedRows[0]=row;
                        select(row);
                    } else {
                        if(e.target.isSelector)
                            return;

                        if(e.ctrlKey) {
                            var exists=false;
                            for(var i=0;i<selectedRows.length;i++) {
                                if(selectedRows[i]==row) {
                                    exists=true;
                                    if(row.selector) {
                                        row.selector.checked=false;
                                    }
                                    row.element.className="gridRow";
                                    selectedRows.splice(i,1);
                                    break;
                                }
                            }
                            if(!exists) {
                                row.element.className="gridRow gridRowCur";
                                if(row.selector) {
                                    row.selector.checked=true;
                                }
                                selectedRows.push(row);
                                select(row);
                            } else
                                select(selectedRows.length!=0?selectedRows[selectedRows.length-1]:null);

                        } else if(e.shiftKey) {

                            for(var i=selectedRows.length-1;i>=0;i--) {
                                if(selectedRows[i].selector) {
                                    selectedRows[i].selector.checked=false;
                                }
                                selectedRows[i].element.className="gridRow";
                                selectedRows.pop();
                            }

                            var start;
                            var end;
                            if(!selectedRow) {
                                start=row.rowIndex;
                                end=row.rowIndex;
                            } else if(selectedRow.rowIndex>=row.rowIndex) {
                                start=row.rowIndex;
                                end=selectedRow.rowIndex;
                            } else {
                                start=selectedRow.rowIndex;
                                end=row.rowIndex;
                            }
                            for(var i=start;i<=end;i++) {
                                rows[i].element.className="gridRow gridRowCur";
                                if(rows[i].selector) {
                                    rows[i].selector.checked=true;
                                }
                                selectedRows.push(rows[i]);
                            }
                            select(rows[end]);
                        } else {
                            for(var i=selectedRows.length-1;i>=0;i--) {
                                if(selectedRows[i].selector) {
                                    selectedRows[i].selector.checked=false;
                                }
                                selectedRows[i].element.className="gridRow";
                                selectedRows.pop();
                            }
                            row.element.className="gridRow gridRowCur";
                            if(row.selector) {
                                row.selector.checked=true;
                            }
                            selectedRows.push(row);
                            select(row);
                        }
                    }
                });

            var row={
                data: rowData,
                element: gridRow,
                cells: [],
                remove: function () {
                    if(selectedRow==this)
                        selectedRow=null;

                    for(var i=0,j=selectedRows.length;i<j;i++) {
                        if(selectedRows[i]==this) {
                            selectedRows.splice(i,1);
                            break;
                        }
                    }

                    this.element.parentNode.removeChild(this.element);
                    data.splice(this.rowIndex,1);
                    rows.splice(this.rowIndex,1);

                    for(var i=this.rowIndex;i<rows.length;i++) {
                        rows[i].rowIndex=i;
                        for(var j=0;j<columns.length;j++) {
                            if(columns[j].type=="number")
                                rows[i].cells[j].rowNumber(i+1);
                        }
                    }
                }
            };
            rows.push(row);

            if(config.children) {
                var childGrid;
                var childContainer=$("DIV",content).css("gridChildNon");
                row.childContainer=childContainer;
                row.children=[];
                configCol[0].isSign=true;

                $.each(config.children,function (child) {
                    if(typeof child.custom==="function") {
                        child.custom(childContainer,rowData,row);
                    } else {
                        child.id=$("DIV",childContainer);
                        if(child.dataName)
                            child.data=rowData[child.dataName];
                        childGrid=new $.Grid($.extend({},child));
                        childGrid.parentRow=row;
                        row.children.push(childGrid);
                    }
                });
            }

            for(var i=0,j=columns.length;i<j;i++) {
                appendCell(row,columns[i]);
            }
        };
        var appendCell=function (row,column) {
            var cellWidth=column.width=="auto"?"auto":(column.width+"%");
            var gridCell=$("LI",row.element).css("gridCell").css({ width: cellWidth,height: config.rowHeight+"px" });
            var item;
            var itemParent;
            var cellValue;
            var cell;

            if(column.style)
                gridCell.css(column.style);
            if(column.align)
                gridCell.css({ "textAlign": column.align });

            if(column.isSign) {
                itemParent=$("DIV",gridCell).css("gridPlus").on("click",function (e) {
                    if(e.target==this) {
                        if(this.minus) {
                            this.css("gridPlus");
                            row.childContainer.css("gridChildNon");

                        } else {
                            this.css("gridMinus");
                            row.childContainer.css("gridChildCon");

                            if(row.onopen)
                                row.onopen(row,data,container)
                        }
                        this.minus=!this.minus;
                    }
                }).prop("minus",false);
                row.expand=function () {
                    if(itemParent.minus)
                        itemParent.on("click");
                };
                row.shrink=function () {
                    if(!itemParent.minus)
                        itemParent.on("click");
                };
            }
            else
                itemParent=gridCell;

            item=config.rowHeight==20?$("DIV",itemParent).css("line-height:20px;height:20px;overflow:hidden;word-break:break-all;word-wrap:break-all;"+(column.isSign?"float:left;text-align:left;width:100%;":"padding:0px 6px;")):column.type=="textbox"?$("DIV",itemParent):$("TD",$("TR",$("TBODY",$("TABLE",itemParent).css("gridCellItem")))).css("table-layout:fixed;word-break:break-all;word-wrap:break-all;");

            if(isUndefined(column.columnName))
                cellValue=column.defaultValue||"";
            else {
                cellValue=isUndefined(row.data[column.columnName])?(column.defaultValue||""):row.data[column.columnName];

                if(typeof column.format==="function")
                    cellValue=column.format(cellValue);
            }

            cell={
                column: column,
                row: row,
                grid: _self,
                cell: gridCell,
                element: gridCell,
                html: function (a) {
                    if(typeof a==="undefined")
                        return item.innerHTML;
                    item.innerHTML=a;
                },
                append: function (a) {
                    item.appendChild(a);
                },
                val: function (a,b) {
                    if(typeof a=="undefined")
                        return row.data[column.columnName];
                    row.data[column.columnName]=a;

                    if(typeof cell.onchange=="function")
                        cell.onchange(a);
                }
            };
            row.cells.push(cell);
            gridCell.cell=cell;
            cell.value=cell.val;

            switch(column.type) {
                case "textbox":
                    cell.textbox=$("TEXTAREA",item)
                        .css("gridCellTextBox")
                        .css({ height: config.rowHeight+"px" })
                        .prop("value",cellValue)
                        .prop("validate",function () {
                            var error=false;

                            if(column.emptyAble===false&&this.value=="") {
                                error=column.emptyText||(column.header+"不可为空！");
                            } else if(column.regex&&!column.regex.test(this.value)) {
                                error=column.regexText||(column.header+"格式错误！");
                            }

                            if(!!error)
                                gridCell.css("gridCell gridCellErr").title=error;
                            else
                                gridCell.css("gridCell").title="";

                            return !error;
                        })
                        .on("blur",function () {
                            if(this.validate()&&isFunction(column.onchange))
                                column.onchange(this,cell,row.data)
                        });

                    cell.onchange=function (a) {
                        cell.textbox.value=a;
                        cell.textbox.validate();
                    };
                    break;

                case "checkbox":
                    cell.checkbox=$("CHECKBOX",item)
                        .attr("checked",!!cellValue);

                    cell.onchange=function (a) {
                        cell.checkbox.checked=!!a;
                    };
                    break;

                case "custom":
                    if(typeof column.custom==="function")
                        column.custom(cell,row.data);
                    break;

                case "selector":
                    cell.selector=$(config.multiselect?"CHECKBOX":"RADIO",item,"__gs_"+guid)
                        .prop({ "isSelector": true,"checked": false });

                    if(config.multiselect)
                        cell.selector.on("click",function () {
                            if(this.checked) {
                                row.element.className="gridRow gridRowCur";
                                selectedRows.push(row);
                                select(row);
                            }
                            else {
                                for(var i=0;i<selectedRows.length;i++) {
                                    if(selectedRows[i]==row) {
                                        row.element.className="gridRow";
                                        selectedRows.splice(i,1);
                                        break;
                                    }
                                }
                                if(selectedRow==row) {
                                    if(selectedRows.length!=0) {
                                        select(selectedRows[selectedRows.length-1]);
                                    }
                                }
                            }
                        });

                    row.selector=cell.selector;
                    break;

                default:
                    item.innerHTML=cellValue;

                    cell.onchange=function (a) {
                        item.innerHTML=a;
                    };
                    break;
            }

        };
        var select=!isFunction(config.onRowSelect)?
            function (row) {
                selectedRow=row;
            } :
            function (row) {
                selectedRow=row;
                if(row)
                    config.onRowSelect(row);
            };
        var bindContextMenu=function (contextMenuConfig) {
            var contextMenuItems=contextMenuConfig.items;
            var length=contextMenuItems.length;
            var item;
            var maxWidth=0;

            if(menu==null) {
                contextMenu=$("DIV",document.body).css("contextmenu");
                menu=$("DIV",contextMenu).css("contextmenu_con");
                gridInside.on("contextmenu",function (e) {
                    var target=e.target;

                    if($.ie)
                        document.fireEvent("onclick");

                    contextMenuCell==null;

                    while(target&&target!=gridInside) {
                        if(typeof target.cell=="object") {
                            var docClick=document.onclick;
                            contextMenuCell=target.cell;
                            contextMenu.style.cssText="visibility:visible;left:"+e.pageX+"px;top:"+e.pageY+"px;";
                            document.onclick=function () {
                                contextMenu.style.visibility="hidden";
                                document.onclick=docClick;
                            };
                            break;
                        }
                        target=target.parentNode;
                    }

                    return false;
                });
            } else
                menu.innerHTML="";

            for(var i=0;i<length;i++) {
                if(contextMenuItems[i]=="-")
                    $("SPAN",menu);
                else {
                    item=$("A",menu)
                    .css("float:left")
                    .prop("href","#")
                    .html(contextMenuItems[i].text)
                    .on("click",(function (i) {
                        return function () {
                            if(typeof contextMenuItems[i].handler=="function")
                                contextMenuItems[i].handler(contextMenuCell);
                            return false;
                        }
                    })(i));

                    maxWidth=Math.max(maxWidth,item.offsetWidth);
                    item.cssText("");
                }
            }
            menu.style.width=maxWidth+"px";
        };

        var container=$(config.id);
        var grid=$("DIV",container).css("grid").css("width:100%");
        var gridInside=$("DIV",grid).css("gridInside");
        var header=$("OL",$("DIV",gridInside).css("gridHeaderCon")).css("gridHeader");
        var content=$("DIV",gridInside).css("gridCon");
        var defaultColumnsWidth=0;

        if(isUndefined(config.rowHeight))
            config.rowHeight=20;

        header.onselectstart=function (e) {
            return false;
        };
        if(config.pageEnabled) {
            gridPage=$("DIV",grid)
                .html("<span>共0条数据</span>")
                .css("page");
        }

        if(configCol) {
            var fixedWidth=0;
            var len=configCol.length;
            var columnsWidth=0;

            $.each(configCol,function (column) {
                if(typeof column.height==="undefined")
                    column.height=config.rowHeight;

                column.width=Math.max(column.width,1);
                defaultColumnsWidth+=column.width;
                column.defaultWidth=column.width;
            });

            len--;
            for(var i=0;i<len;i++) {
                column=configCol[i];
                column.width=Math.round(100*column.width/defaultColumnsWidth);
                columnsWidth+=column.width;
                addColumn(column);
            }
            configCol[len].width=$.ie?"auto":(100-columnsWidth);
            addColumn(configCol[len]);
        }

        if(config.url)
            load(config.url,config.params);
        else if(config.data)
            loadData(config.data);

        if(config.contextMenu)
            bindContextMenu(config.contextMenu);

        if(typeof config.handler=="function") {
            gridInside.on("mouseup",function (e) {
                var target=e.target;
                while(target&&target!=gridInside) {
                    if(typeof target.cell=="object") {
                        config.handler(target.cell);
                        break;
                    }
                    target=target.parentNode;
                }
            });
        }

        this.acceptChanges=function () {
            var flag=true;
            var validFlag;
            $.each(rows,function (row) {
                $.each(row.cells,function (cell) {
                    if(cell.column.type=="textbox") {
                        validFlag=cell.textbox.validate();
                        if(validFlag)
                            row.data[cell.column.columnName]=cell.textbox.value;
                        flag&=validFlag
                    } else if(cell.column.type=="checkbox")
                        row.data[cell.column.columnName]=cell.checkbox.checked;
                });
            });
            return flag;
        };
        this.container=container;
        this.data=data;
        this.columns=columns;
        this.rows=rows;
        this.getSelectedRow=function () { return selectedRow; };
        this.getSelectedRows=function () { return selectedRows; };
        this.load=load;
        this.loadData=loadData;
        this.addRow=appendData;
        this.showMsg=showMsg;
        this.getJsonResult=function () {
            return jsonResult;
        };

        guid++;
        return this;
    };

})();
