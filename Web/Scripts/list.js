(function () {
    var defaultConfig={
        rowHeight: 20
    };
    var extend=function (a,b) {
        for(var i in a) {
            if(typeof b[i]==="undefined")
                b[i]=a[i];
        }
        return b;
    };

    $.list=function (config) {
        if(typeof config.rowHeight==="undefined")
            config.rowHeight=defaultConfig.rowHeight;

        var selectedRows=[],
            data=[],
            selectedRow=null,
            rows=[],
            columns=[];

        var mainWidth=0;

        var container=document.getElementById(config.id),
            fragment=document.createDocumentFragment(),
            datalistContainer=document.createElement("DIV"),
            datalist=document.createElement("DIV"),
            main=document.createElement("DIV"),
            header=document.createElement("OL"),
            content=document.createElement("DIV");

        fragment.appendChild(datalistContainer);
        datalistContainer.appendChild(datalist);
        datalist.appendChild(main);
        main.appendChild(header);
        main.appendChild(content);

        datalistContainer.className="datalist";
        datalistContainer.style.cssText="-moz-user-select:-moz-none;width:"+(!config.width?"100%":config.width+"px");
        datalistContainer.onselectstart=function () {
            return /input|textarea/i.test(window.event.srcElement.tagName);
        };
        datalist.style.cssText="overflow:auto;width:100%;height:"+(!config.height?"auto":config.height+"px");

        main.className="list_main";
        header.className="list_tit";
        content.className="list_con";

        function reset() {
            while(data.length) {
                data.pop();
            }
            while(rows.length!=0) {
                rows.pop();
            }
        }

        function showMsg(msg) {
            content.innerHTML="<div style='text-align:center;line-height:24px'>"+msg+"</div>";
        };

        var baseParams,page;
        if(config.pageEnabled) {
            page=$.createElement({
                tagName: "DIV",
                parentNode: datalistContainer,
                innerHTML: "<span>共0条数据</span>",
                className: "page"
            });
        }
        function load(url,params,callback) {
            if(!url&&!params) {
                url=config.url;
                if(typeof baseParams=="undefined")
                    baseParams={};
            }
            else {
                config.url=url;
                baseParams=params||{};
            }
            if(config.pageEnabled) {
                baseParams.page=baseParams.page||1;
                baseParams.pageSize=baseParams.pageSize||10;
            }
            showMsg("正在载入数据...");
            $.post(url,function (res) {
                if(res.success) {
                    loadData(res.data);
                    if(config.pageEnabled) {
                        $.page(baseParams.page,baseParams.pageSize,res.total,page,function (pageIndex) {
                            baseParams.page=pageIndex;
                            load();
                        });
                    }
                } else {
                    reset();
                    showMsg(res.msg);
                }
                if(typeof callback==="function")
                    callback(res);
            },baseParams);
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
            var li=document.createElement("LI"),
                text=document.createElement("A"),
                em=document.createElement("EM");

            if(columns.length!=0)
                columns[columns.length-1].element.className="";

            mainWidth+=column.width;
            main.style.width=mainWidth+"px";

            header.appendChild(li);
            li.appendChild(text);
            li.appendChild(em);
            li.className="last";

            li.width=column.width;
            li.style.cssText="width:"+column.width+"px;";
            text.style.cssText="width:"+(column.width-8)+"px";
            text.innerHTML=column.header||"";

            column.columnIndex=columns.length;
            column.element=li;

            em.onmousedown=(function (oResize,oText,columnIndex) {
                return function (e) {
                    e=window.event||e;
                    var startPX=e.pageX||e.clientX;
                    var oldWidth=oResize.width;
                    var oldMainWidth=mainWidth;
                    document.body.style.cursor="e-resize";
                    var bodyOnMouseUp=document.body.onmouseup;
                    var bodyOnMouseMove=document.body.onmousemove;
                    document.body.onmousemove=function (e) {
                        e=window.event||e;
                        var cPX=e.pageX||e.clientX;
                        var fix=cPX-startPX;
                        var orw=oldWidth+fix
                        if(orw>8) {
                            oResize.width=orw;
                            oResize.style.width=oResize.width+"px";
                            oText.style.width=oResize.width-8+"px";

                            mainWidth=oldMainWidth+fix;
                            main.style.width=mainWidth+"px";
                            window.setTimeout(function () {
                                for(var i=0;i<rows.length;i++) {
                                    rows[i].cells[columnIndex].cell.style.width=oResize.width+"px";
                                }
                            },10);
                        }
                    }
                    document.body.onmouseup=function () {
                        document.body.style.cursor="default";
                        document.body.onmouseup=bodyOnMouseUp;
                        document.body.onmousemove=bodyOnMouseMove;
                    }
                }

            })(li,text,column.columnIndex);

            columns.push(column);
            if(column.sortAble) {
                text.style.cursor="pointer";
                text.onclick=function () {
                    if(config.pageEnabled&&typeof baseParams!=="undefined") {
                        baseParams.sort=column.columnName;

                        this.isAsc=!!this.isAsc;
                        baseParams.isAsc=this.isAsc;
                        this.className=this.isAsc?"":"";

                        load();
                    }

                }
            }

            for(var i=0;i<rows.length;i++) {
                appendCell(rows[i],column);
            }
        };

        function appendData(rowData) {
            appendRow(rowData);
            data.push(rowData);
        }

        function appendRow(rowData) {
            var row={};

            row.element=document.createElement("UL");
            row.element.style.cssText="height:"+(config.rowHeight+2)+"px";
            row.element.className="list_row";
            row.rowIndex=rows.length;
            row.data=rowData;
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

            content.appendChild(row.element);
            row.element.onclick=function (e) {
                e=window.event||e;
                if(!config.multiselect) {
                    if(selectedRow)
                        selectedRow.element.className="list_row";
                    row.element.className="list_row_current";
                    selectedRows[0]=row;
                    selectedRow=row;
                } else {
                    if(e.ctrlKey) {
                        var exists=false;
                        for(var i=0;i<selectedRows.length;i++) {
                            if(selectedRows[i]==row) {
                                exists=true;
                                row.element.className="list_row";
                                selectedRows.splice(i,1);
                                break;
                            }
                        }
                        if(!exists) {
                            row.element.className="list_row_current";
                            selectedRow=row;
                            selectedRows.push(row);
                        } else
                            selectedRow=selectedRows.length!=0?selectedRows[selectedRows.length-1]:null;

                    } else if(e.shiftKey) {
                        for(var i=selectedRows.length-1;i>=0;i--) {
                            selectedRows[i].element.className="list_row";
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
                            rows[i].element.className="list_row_current";
                            selectedRows.push(rows[i]);
                        }
                    } else {
                        for(var i=selectedRows.length-1;i>=0;i--) {
                            selectedRows[i].element.className="list_row";
                            selectedRows.pop();
                        }
                        row.element.className="list_row_current";
                        selectedRows.push(row);
                        selectedRow=row;
                    }
                }
            };

            for(var i=0,j=columns.length;i<j;i++) {
                appendCell(row,columns[i]);
            }
        };

        function appendCell(row,column) {

            var li=document.createElement("LI");
            li.className="list_cell";
            li.style.cssText="width:"+column.width+"px;height:"+column.height+"px";
            row.element.appendChild(li);

            var item=document.createElement("DIV");
            item.style.cssText="margin:4px 4px 2px 4px;width:"+(column.width-8)+"px;height:"+(column.height-6)+"px";

            li.appendChild(item);
            if(typeof row.data[column.columnName]=="undefined")
                row.data[column.columnName]="";

            var cellValue=typeof row.data[column.columnName]==="undefined"?"":row.data[column.columnName];
            var cell={};
            cell.column=column;
            cell.row=row;
            cell.text=item;
            cell.cell=li;

            row.cells.push(cell);
            li.cell=cell;

            if(column.type=="number") {
                li.className="list_cell_num";
                item.innerHTML=rows.length;
                cell.rowNumber=function (a) {
                    if(typeof a==="undefined")
                        return (cell.row.rowIndex+1);
                    item.innerHTML=a;
                }

            } else {

                cell.value=function (a,b) {
                    if(typeof a=="undefined")
                        return row.data[column.columnName];
                    row.data[column.columnName]=a;

                    if(typeof cell.onchange=="function"&&typeof b=="undefined")
                        cell.onchange(a);
                }

                switch(column.type) {
                    case "textbox":
                        li.className="list_cell_txt";

                        cell.textbox=$.createElement({
                            tagName: "TEXTAREA",
                            parentNode: item,
                            value: cellValue,
                            valid: function () {
                                var error=false;

                                if(column.emptyAble!==undefined&&!!column.emptyAble) {
                                    if(this.value=="") {
                                        error=column.emptyText||(column.header+"不可为空！");
                                    }
                                } else if(!!column.validRegex) {
                                    if(!column.validRegex.test(this.value)) {
                                        error=column.validText||(column.header+"不可为空！");
                                    }
                                }

                                if(!!error) {
                                    li.className="list_cell_txt list_cell_err";
                                    li.title=error;
                                } else {
                                    li.className="list_cell_txt";
                                    li.title="";
                                }

                                return !error;
                            },
                            onblur: function () {
                                if(this.valid()) {
                                    cell.value(this.value,true);
                                }
                            }
                        });

                        cell.onchange=function (a) {
                            cell.textbox.value=a;
                            cell.textbox.valid();
                        };
                        break;

                    case "checkbox":
                        cell.checkbox=$.createElement({
                            tagName: "INPUT",
                            parentNode: item,
                            type: "checkbox",
                            checked: !!cellValue,
                            onclick: function () {
                                cell.value(this.checked,true);
                            }
                        });

                        cell.onchange=function (a) {
                            cell.checkbox.checked=!!a;
                        };
                        break;

                    case "custom":
                        if(typeof column.custom==="function")
                            column.custom(cell);
                        break;

                    default:
                        item.innerHTML=cellValue;

                        cell.onchange=function (a) {
                            item.innerHTML=a;
                        };
                        break;
                }
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
                datalist.oncontextmenu=function (e) {
                    if($.isIE)
                        document.fireEvent("onclick");

                    var mouseX,mouseY,target;
                    if(window.event) {
                        e=window.event;
                        mouseX=document.documentElement.offsetTop+e.clientX;
                        mouseY=document.documentElement.offsetLeft+e.clientY;
                        target=e.srcElement;
                    } else {
                        mouseX=e.pageX;
                        mouseY=e.pageY;
                        target=e.target;
                    }
                    contextMenuCell==null;

                    while(target&&target!=datalist) {
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

        if(config.columns) {
            var dw=container.offsetWidth-24,
                w=0,
                len=0;

            for(var i=0,n=config.columns.length;i<n;i++) {
                column=config.columns[i];
                if(typeof column.height==="undefined")
                    column.height=config.rowHeight;

                column.width=Math.max(column.width,24);
                if(column.type!="number")
                    len++;
                w+=column.width+2;
            }
            var fw=0;
            if(w<dw&&len!=0)
                fw=Math.ceil((dw-w)/len);

            for(var i=0,n=config.columns.length;i<n;i++) {
                column=config.columns[i];
                if(column.type!="number")
                    column.width+=fw;
                addColumn(column);
            }
        }

        if(config.url)
            load(config.url,config.params);
        else if(config.data)
            loadData(config.data);

        if(config.contextMenu)
            bindContextMenu(config.contextMenu);

        if(typeof config.handler=="function") {
            datalist.onmouseup=function (e) {
                var target=window.event&&window.event.srcElement||e.target;
                while(target&&target!=datalist) {
                    if(typeof target.cell=="object") {
                        config.handler(target.cell);
                        break;
                    }
                    target=target.parentNode;
                }
            }
        }

        container.appendChild(fragment);

        function validData() {
            var cells,flag=true;
            for(var i=0;i<rows.length;i++) {
                cells=rows[i].cells;
                for(var j=0,n=cells.length;j<n;j++) {
                    if(cells[j].column.type=="textbox") {
                        flag&=cells[j].textbox.valid();
                    }
                }
            }
            return flag;
        }

        return {
            getData: function () { return data },
            columns: columns,
            rows: rows,
            getSelectedRow: function () { return selectedRow; },
            selectedRows: selectedRows,
            load: load,
            loadData: loadData,
            addColumn: addColumn,
            addRow: appendData,
            showMsg: showMsg,
            validData: validData
        }
    };

})();
