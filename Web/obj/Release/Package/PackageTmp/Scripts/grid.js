(function () {

    $.Grid=function (config) {
        if(!$.Grid.guid)
            $.Grid.guid=1;

        var guid=$.Grid.guid++;

        if(typeof config.rowHeight==="undefined")
            config.rowHeight=20;

        var selectedRows=[],
            data=[],
            selectedRow=null,
            rows=[],
            columns=[];

        var container=$(config.id);
        var grid=$("TD",$("TR",$("TBODY",$("TABLE",container).css("grid").css({ "width": "100%" }))));
        var gridInside=document.createElement("DIV");
        grid.appendChild(gridInside);
        gridInside.className="gridInside";

        var header=$("OL",$("TD",$("TR",$("TBODY",$("TABLE",gridInside).css({ "width": "100%" }))))).css({ "width": "100%" }).css("gridHeader");
        header.onselectstart=function (e) {
            return false;
        };

        var content=$("DIV",$("TD",$("TR",$("TBODY",$("TABLE",gridInside).css({ "width": "100%" })))));

        function reset() {
            while(data.length) {
                data.pop();
            }
            while(rows.length!=0) {
                rows.pop();
            }
        }

        function showMsg(msg) {
            $("LI",$("UL",$(content).html("")).css("gridRow")).css("gridCell").css({ width: "100%",lineHeight: "26px",textAlign: "left",textIndent: "1em" }).html(msg);
        };

        var jsonResult=null;
        var baseParams,gridPage;
        if(config.pageEnabled) {
            gridPage=$("DIV",grid)
                .html("<span>共0条数据</span>")
                .css("page");
        }
        function load(url,params,callback) {
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
                jsonResult=res;
                var page;
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
                if(typeof config.callback==="function")
                    config.callback(res,page);
            },baseParams);
        };

        this.getJsonResult=function () {
            return jsonResult;
        };

        function loadData(data) {
            content.innerHTML="";
            reset();

            data=data;

            if(data&&data.length) {
                for(var i=0;i<data.length;i++) {
                    appendRow(data[i]);
                }
            } else {
                showMsg("暂无数据。");
            }
        };

        function addColumn(column) {
            column.columnIndex=columns.length;
            columns.push(column);

            var headerCell=$("LI",header).css({ "width": column.width=="auto"?"auto":(column.width+"%") });
            if(column.align)
                headerCell.css({ "textAlign": column.align });

            column.element=headerCell;

            var headerText=$("A",headerCell).html(column.header||"");
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
                }
            }

            var headerResizer=document.createElement("EM");
            headerText.appendChild(headerResizer);

            headerResizer.onmousedown=function (e) {
                e=$E(e);

                var startPX=e.pageX;
                var oldWidth=headerCell.offsetWidth;
                var oldGridWidth=grid.offsetWidth-1;

                document.body.style.cursor="e-resize";
                var bodyOnMouseUp=document.body.onmouseup;
                var bodyOnMouseMove=document.body.onmousemove;
                document.body.onmousemove=function (e) {
                }
                document.body.onmouseup=function (e) {
                    e=$E(e);
                    var changedWidth=e.pageX-startPX;

                    document.body.style.cursor="default";
                    document.body.onmouseup=bodyOnMouseUp;
                    document.body.onmousemove=bodyOnMouseMove;
                }
            }
        };

        function appendData(rowData) {
            appendRow(rowData);
            data.push(rowData);
        }

        var select=typeof config.onRowSelect!=="function"?
            function select(row) {
                selectedRow=row;
            } :
            function select(row) {
                selectedRow=row;
                if(row)
                    config.onRowSelect(row);
            };

        function appendRow(rowData) {

            var gridRow=document.createElement("UL");
            content.appendChild(gridRow);
            gridRow.style.cssText="width:100%;height:"+(config.rowHeight)+"px";
            gridRow.className="gridRow";

            var row={};
            row.data=rowData;
            row.element=gridRow;
            row.rowIndex=rows.length;
            row.cells=[];
            row.remove=function () {
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
            };

            rows.push(row);

            gridRow.onclick=function (e) {
                e=$E(e);

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

                        var start,end;
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
            };

            if(config.children) {
                var childContainer=$("DIV",content).css("gridChildNon");
                row.childContainer=childContainer;
                row.children=[];
                config.columns[0].isSign=true;

                $.each(config.children,function (child) {
                    if(typeof child.custom==="function") {
                        child.custom(childContainer,rowData,row);
                    }
                    else {
                        child.id=$("DIV",childContainer);
                        if(child.dataName)
                            child.data=rowData[child.dataName];
                        row.children.push(new $.Grid($.extend({},child)));
                    }
                });
            }

            for(var i=0,j=columns.length;i<j;i++) {
                appendCell(row,columns[i]);
            }
        };

        function appendCell(row,column) {
            var cellWidth=column.width=="auto"?"auto":(column.width+"%");
            var gridCell=$("LI",row.element).css("gridCell").css({ width: cellWidth,height: config.rowHeight+"px" });
            if(column.style)
                gridCell.css(column.style);
            if(column.align)
                gridCell.css({ "textAlign": column.align });

            var item;
            var itemParent;
            if(column.isSign) {
                itemParent=$("DIV",gridCell).css("gridCellSign");
                $("SPAN",itemParent).css("gridPlus").on("click",function () {
                    if(this.minus) {
                        this.css("gridPlus");
                        row.childContainer.css("gridChildNon");
                    } else {
                        this.css("gridMinus");
                        row.childContainer.css("gridChildCon");
                    }
                    this.minus=!this.minus;
                }).css({ "height": config.rowHeight+"px" }).css("gridPlus").prop("minus",false);
            }
            else
                itemParent=gridCell;

            item=column.type=="textbox"?$("DIV",itemParent):
            $("TD",$("TR",$("TBODY",$("TABLE",itemParent).css("gridCellItem"))));
            item.style.cssText="table-layout:fixed;word-break:break-all;word-wrap:break-all;";

            if(typeof row.data[column.columnName]=="undefined")
                row.data[column.columnName]="";

            var cellValue=typeof row.data[column.columnName]==="undefined"?"":row.data[column.columnName];
            if(typeof column.format==="function")
                cellValue=column.format(cellValue);

            var cell={
                column: column,
                row: row,
                cell: gridCell,
                html: function (a) {
                    if(typeof a==="undefined")
                        return item.innerHTML;
                    item.innerHTML=a;
                },
                append: function (a) {
                    item.appendChild(a);
                }
            };
            row.cells.push(cell);
            gridCell.cell=cell;

            cell.value=function (a,b) {
                if(typeof a=="undefined")
                    return row.data[column.columnName];
                row.data[column.columnName]=a;

                if(typeof cell.onchange=="function")
                    cell.onchange(a);
            }

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
                        .prop("isSelector",true)
                        .prop("checked",false)

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

        var contextMenu=null,
            menu=null,
            contextMenuCell=null;

        function bindContextMenu(contextMenuConfig) {
            var contextMenuItems=contextMenuConfig.items,
                length=contextMenuItems.length,
                item;
            if(menu==null) {
                contextMenu=document.createElement("DIV");
                contextMenu.className="contextmenu";
                document.body.appendChild(contextMenu);
                menu=document.createElement("DIV");
                menu.className="contextmenu_con";
                contextMenu.appendChild(menu);
                gridInside.oncontextmenu=function (e) {
                    if($.isIE)
                        document.fireEvent("onclick");

                    var mouseX,mouseY,target;
                    if(window.event) {
                        e=window.event;
                        mouseX=e.clientX+$.getScrollLeft();
                        mouseY=e.clientY+$.getScrollTop();
                        target=e.srcElement;
                    } else {
                        mouseX=e.pageX;
                        mouseY=e.pageY;
                        target=e.target;
                    }
                    contextMenuCell==null;

                    while(target&&target!=gridInside) {
                        if(typeof target.cell=="object") {
                            contextMenuCell=target.cell;
                            contextMenu.style.cssText="visibility:visible;left:"+mouseX+"px;top:"+mouseY+"px;";
                            var docClick=document.onclick;
                            document.onclick=function () {
                                contextMenu.style.visibility="hidden";
                                document.onclick=docClick;
                            }
                            break;
                        }
                        target=target.parentNode;
                    }

                    return false;
                }
            } else
                menu.innerHTML="";

            var maxWidth=0;
            for(var i=0;i<length;i++) {
                if(contextMenuItems[i]=="-") {
                    item=document.createElement("SPAN");
                    menu.appendChild(item);
                } else {
                    item=document.createElement("A");
                    menu.appendChild(item);

                    item.style.cssText="float:left";
                    item.href="#";
                    item.innerHTML=contextMenuItems[i].text;
                    item.onclick=(function (i) {
                        return function () {
                            if(typeof contextMenuItems[i].handler=="function")
                                contextMenuItems[i].handler(contextMenuCell);
                            return false;
                        }
                    })(i);

                    maxWidth=Math.max(maxWidth,item.offsetWidth);
                    item.style.cssText="";
                }
            }
            menu.style.width=maxWidth+"px";
        };
        var defaultColumnsWidth=0;
        if(config.columns) {

            $.each(config.columns,function (column) {
                if(typeof column.height==="undefined")
                    column.height=config.rowHeight;

                column.width=Math.max(column.width,1);
                defaultColumnsWidth+=column.width;
                column.defaultWidth=column.width;
            });

            var fixedWidth=0,len=config.columns.length;
            len--;
            var columnsWidth=0;
            for(var i=0;i<len;i++) {
                column=config.columns[i];
                column.width=Math.round(100*column.width/defaultColumnsWidth);
                columnsWidth+=column.width;
                addColumn(column);
            }

            config.columns[len].width=$.ie?"auto":(100-columnsWidth);
            addColumn(config.columns[len]);
        }

        if(config.url)
            load(config.url,config.params);
        else if(config.data)
            loadData(config.data);

        if(config.contextMenu)
            bindContextMenu(config.contextMenu);

        if(typeof config.handler=="function") {
            gridInside.onmouseup=function (e) {
                var target=window.event&&window.event.srcElement||e.target;
                while(target&&target!=gridInside) {
                    if(typeof target.cell=="object") {
                        config.handler(target.cell);
                        break;
                    }
                    target=target.parentNode;
                }
            };
        }

        this.acceptChanges=function () {
            var flag=true,validFlag;
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

        this.data=data;
        this.columns=columns;
        this.rows=rows;
        this.getSelectedRow=function () { return selectedRow; };
        this.getSelectedRows=function () { return selectedRows; };
        this.load=load;
        this.loadData=loadData;
        this.addRow=appendData;
        this.showMsg=showMsg;

    };

})();
