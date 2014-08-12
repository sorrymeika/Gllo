(function (win,doc) {
    win.isUndefined=function (v) {
        return typeof v==="undefined"
    };
    win.isRegExp=function (v) {
        return Object.prototype.toString.call(v)=="[object RegExp]";
    };
    win.isArray=function (v) {
        return Object.prototype.toString.apply(v)==='[object Array]'
    };
    win.isFunction=function (v) {
        return typeof v==="function"
    };
    win.isString=function (v) {
        return typeof v==="string"
    };
    win.isElement=function (v) {
        return typeof v==="object"&&(v.nodeType===1||v.nodeType===11)
    };

    Date.fromSeconds=function (s) {
        var d=new Date(0);
        d.setSeconds(s);
        return d;
    };

    Date.prototype.format=function (f) {
        var y=this.getFullYear()+"",M=this.getMonth()+1,d=this.getDate(),H=this.getHours(),m=this.getMinutes(),s=this.getSeconds(),mill=this.getMilliseconds()+"0000";

        return f.replace(/\y{4}/,y)
            .replace(/y{2}/,y.substr(2,2))
            .replace(/M{2}/,$.pad(M,2))
            .replace(/M/,M)
            .replace(/d{2,}/,$.pad(d,2))
            .replace(/d/,d)
            .replace(/H{2,}/,$.pad(H,2))
            .replace(/H/,H)
            .replace(/m{2,}/,$.pad(m,2))
            .replace(/m/,m)
            .replace(/s{2,}/,$.pad(s,2))
            .replace(/s/,s)
            .replace(/f+/,function (w) {
                return mill.substr(0,w.length)
            })
    };

    Array.prototype.contains=function (item) {
        for(var i=0,n=this.length;i<n;i++) {
            if(this[i]==item)
                return true;
        }
        return false;
    };

    Array.prototype.indexOf=function (item) {
        for(var i=0,n=this.length;i<n;i++) {
            if(this[i]==item)
                return i;
        }
        return -1;
    };

    var addEvent,removeEvent,guid=1;
    if(win.addEventListener) {
        addEvent=function (o,e,h) {
            o.addEventListener(e,h,false);
        };
        removeEvent=function (o,e,h) {
            o.removeEventListener(e,h,false);
        };
    } else if(win.attachEvent) {
        addEvent=function (o,e,h) {
            if(!h.$$guid) h.$$guid=guid++;
            if(!o.events) o.events={};
            var handlers=o.events[e];
            if(!handlers)
                handlers=o.events[e]={};
            var handler=handlers[h.$$guid];
            if(!handler) {
                handler=handlers[h.$$guid]=function () {
                    h.call(o,$E())
                };
                o.attachEvent('on'+e,handler)
            }
        };
        removeEvent=function (o,e,h) {
            if(o.events&&o.events[e]) {
                o.detachEvent("on"+e,o.events[e][h.$$guid]);
                delete o.events[e][h.$$guid];
            }
        };
    }
    win.on=function (e,h) {
        addEvent(this,e,h);
    };

    var _$$={};
    win.$$=function (o) {
        if(isString(o)) {
            var a;
            if(a=_$$[o])
                return a;

            _$$[o]=a=doc.getElementsByName(o);
            o=a;
        }
        o.value=function (v) {
            if(isUndefined(v)) {
                var res=[];
                $.each(this,function (item) {
                    if(item.checked)
                        res.push(item.value)
                });
                return res.join(", ")
            }
            var cks=[];
            v=(v+"").split(", ");
            $.each(this,function (item) {
                item.checked=false;
                $.each(v,function (j) {
                    if(item.value==j) {
                        cks.push(item);
                        return false;
                    }
                });
            });

            $.each(cks,function (i) {
                i.checked=true
            });
        };

        o.on=function (e,h) {
            $.each(this,function (item) {
                addEvent(item,e,h)
            });
        };

        return o;
    };

    win.$E=function (e) {
        if(e) return e;
        e=win.event;
        e.pageX=e.clientX+$.getScrollLeft();
        e.pageY=e.clientY+$.getScrollTop();
        e.target=event.srcElement;
        e.stopPropagation=function () { this.cancelBubble=true; };
        e.preventDefault=function () { this.returnValue=false; };
        if(e.type=="mouseout") {
            e.relatedTarget=e.toElement;
        } else if(event.type=="mouseover") {
            e.relatedTarget=e.fromElement;
        }
        return e;
    };

    var inputTypes=["BUTTON","SUBMIT","CHECKBOX","RADIO","PASSWORD","FILE","HIDDEN","IMAGE"];
    function create(a,b) {
        var c;
        if(c=inputTypes.contains(a.toUpperCase())) {
            c=a;
            a="INPUT";
        }
        if(b) {
            try {
                a=doc.createElement("<"+a+" name='"+b+"'>");
            } catch(e) {
                a=doc.createElement(a);
                a.name=b;
            }
        }
        else
            a=doc.createElement(a);

        if(c)
            a.type=c;
        return a;
    };

    var _$={};
    win.$=function () {
        var arg=arguments,o=arg[0],a;
        if(isString(o)) {
            if(a=arg[1]) {
                o=create(o,arg[2]);
                if(isElement(a))
                    a.appendChild(o);
            }
            else {
                if(a=_$[o])
                    return a;

                b=doc.getElementById(o);
                if(!b)
                    return b;
                _$[o]=b;
                o=b;
            }
        }
        else if(isFunction(o.on))
            return o;

        o.on=function (e,h) {
            if(isUndefined(h)) {
                if(o.fireEvent)
                    o.fireEvent("on"+e);
                else {
                    var evt=doc.createEvent('HTMLEvents');
                    evt.initEvent(e,true,true);
                    o.dispatchEvent(evt);
                }
            }
            else {
                addEvent(this,e,h);
                return this;
            }
        };

        o.attr=function (a,b) {
            if(!isUndefined(b))
                this.setAttribute(a,b);
            else if(isString(a))
                return this.getAttribute(a);
            else
                for(b in a)
                    this.setAttribute(b,a[b]);
            return this;
        };

        o.prop=function (a,b) {
            if(!isUndefined(b))
                this[a]=b;
            else if(isString(a))
                return this[a];
            else
                for(b in a)
                    this[b]=a[b];
            return this;
        };

        o.html=function (a) {
            if(!isUndefined(a)) {
                this.innerHTML=a;
                return this;
            }
            return this.innerHTML;
        };

        o.css=function (css,a) {
            if(isString(css)) {
                if(/\:/.test(css))
                    this.style.cssText=!a?this.style.cssText.replace(/\s*;\s*$/,"")+";"+css:css;
                else
                    this.className=css;
            }
            else if(isUndefined(css))
                return this.className.split(" ");
            else if(isArray(css))
                this.className=css.join(" ");
            else {
                for(var name in css) {
                    var value=css[name];
                    if(name=="opacity"&&$.ie)
                        this.style.filter=(this.currentStyle.filter||"").replace(/alpha\([^)]*\)/,"")+"alpha(opacity="+value*100+")";
                    else if(name=="float")
                        this.style[$.ie?"styleFloat":"cssFloat"]=value;
                    else
                        this.style[name]=value;
                }
            }
            return this;
        };

        var ct=o.contains;
        o.contains=ct?isUndefined(ct.call)?function (b) {
            return o!=b&&ct(b);
        } :function (b) {
            return o!=b&&ct.call(this,b);
        } :function (b) {
            return !!(o.compareDocumentPosition(b)&16);
        };

        o.remove=function (a,b) {
            if(isString(a))
                removeEvent(this,a,b);
            else {
                if(tmp=o.id&&_$[tmp])
                    delete _$[tmp];
                o.parentNode.removeChild(o);
            }
        };

        o.appendTo=function (p) {
            p.appendChild(this);
            return this;
        };

        o.appendText=function (a) {
            this.appendChild(doc.createTextNode(a));
            return this;
        };

        o.getCurrentStyle=doc.defaultView?
                function () { return doc.defaultView.getComputedStyle(this,null); }
                :function () { return this.currentStyle; };

        o.rect=function () {
            var node=this,left=0,top=0,right=0,bottom=0;
            if(!node.getBoundingClientRect||$.ie8) {
                while(node) { left+=node.offsetLeft,top+=node.offsetTop;node=node.offsetParent; };
                right=left+node.offsetWidth;bottom=top+node.offsetHeight;
            } else {
                var rect=node.getBoundingClientRect();
                left=right=$.getScrollLeft();top=bottom=$.getScrollTop();
                left+=rect.left;right+=rect.right;
                top+=rect.top;bottom+=rect.bottom;
            }
            return { "left": left,"top": top,"right": right,"bottom": bottom };
        };

        return o;
    };

    $.each=function (a,f) {
        for(var i=0,n=a.length;i<n;i++) {
            if(f(a[i],i)===false)
                break;
        }
    };

    (function (ua) {
        var b={
            msie: /msie/.test(ua)&&!/opera/.test(ua),
            opera: /opera/.test(ua),
            webkit: /webkit/.test(ua),
            firefox: /firefox/.test(ua),
            chrome: /chrome/.test(ua)
        };
        b.safari=b.webkit&&!b.chrome;
        var vMark="";
        for(var i in b) {
            if(b[i]) {
                vMark=i;
            }
        }
        if(b.safari)
            vMark="version";

        var bv=RegExp("(?:"+vMark+")[\\/: ]([\\d.]+)").test(ua)?RegExp.$1:"0";
        $.bv=bv;
        $.webkit=b.webkit;
        $.ff=b.firefox;
        $.ie=b.msie;
        $.ie6=b.msie&&parseInt(bv)==6;
        $.ie7=b.msie&&parseInt(bv)==7;
        $.ie8=b.msie&&parseInt(bv)==8;
        $.ie9=b.msie&&parseInt(bv)==9;

    })(win.navigator.userAgent.toLowerCase());

    $.getScrollTop=function () {
        return doc.documentElement.scrollTop||doc.body.scrollTop;
    };

    $.getScrollLeft=function () {
        return doc.documentElement.scrollLeft||doc.body.scrollLeft;
    };

    $.getDocRect=function () {
        var body=doc.compatMode!='BackCompat'?doc.documentElement:doc.body;
        return {
            left: $.getScrollLeft(),
            top: $.getScrollTop(),
            width: Math.max(body.clientWidth,body.scrollWidth),
            height: Math.max(body.clientHeight,body.scrollHeight),
            clientWidth: body.clientWidth,
            clientHeight: body.clientHeight
        };
    };

    $.getQueryString=function (name) {
        var result=win.location.search.substr(1).match(new RegExp("(^|&)"+name+"=([^&]*)(&|$)","i"));
        if(result!=null) return decodeURIComponent(result[2]);
        return null
    };

    $.Cookie={
        get: function (name) {
            var res=doc.cookie.match(new RegExp("(^| )"+name+"=([^;]*)(;|$)"));
            if(res!=null)
                return unescape(res[2]);
            return null;
        },
        set: function (name,value,days) {
            var exp=new Date();
            exp.setTime(exp.getTime()+(days||30)*24*60*60*1000);
            doc.cookie=name+"="+escape(value)+";expires="+exp.toGMTString()+";path=/";
        },
        remove: function (name) {
            var v=this.get(name);
            if(v!=null)
                this.set(name,v,-1);
        }
    };

    $.open=function (url,w,h,t) {
        win.open(url,t||"_blank","height="+h+",width="+w+",top="+(screen.availHeight-h)/2+",left="+(screen.availWidth-w)/2+",toolbar=no,menubar=no,scrollbars=yes,resizable=yes,location=no,status=no");
    };

    $.extend=function () {
        var target=arguments[0]||{},i=1,length=arguments.length,deep=true,options;
        if(typeof target==="boolean") {
            deep=target;
            target=arguments[1]||{};
            i=2;
        }
        if(typeof target!=="object"&&Object.prototype.toString.call(target)!="[object Function]")
            target={};
        for(;i<length;i++) {
            if((options=arguments[i])!=null)
                for(var name in options) {
                    var src=target[name],copy=options[name];
                    if(target===copy)
                        continue;
                    if(deep&&copy&&typeof copy==="object"&&!copy.nodeType&&!isRegExp(copy)) {
                        target[name]=arguments.callee(deep,src||(isArray(copy)?[]:{}),copy);
                    }
                    else if(copy!==undefined)
                        target[name]=copy;
                }

        }
        return target;
    };

    (function () {
        function f(n) {
            return $.pad(n,2)
        };
        if(!isFunction(Date.prototype.toJSON)) {
            Date.prototype.toJSON=function (key) {
                return isFinite(this.valueOf())?this.getUTCFullYear()+'-'+f(this.getUTCMonth()+1)+'-'+f(this.getUTCDate())+' '+f(this.getUTCHours())+':'+f(this.getUTCMinutes())+':'+f(this.getUTCSeconds()):null
            };
            String.prototype.toJSON=Number.prototype.toJSON=Boolean.prototype.toJSON=function (key) {
                return this.valueOf()
            }
        }
        var cx=/[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
        escapable=/[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
        meta={
            '\b': '\\b',
            '\t': '\\t',
            '\n': '\\n',
            '\f': '\\f',
            '\r': '\\r',
            '"': '\\"',
            '\\': '\\\\'
        };
        function quote(string) {
            escapable.lastIndex=0;
            return escapable.test(string)?'"'+string.replace(escapable,
            function (data) {
                var c=meta[data];
                return isString(c)?c:'\\u'+('0000'+data.charCodeAt(0).toString(16)).slice(-4)
            })+'"':'"'+string+'"'
        };
        function str(key,holder) {
            var i,
            k,
            v,
            length,
            partial,
            value=holder[key];
            if(value&&typeof value==='object'&&isFunction(value.toJSON)) {
                value=value.toJSON(key)
            }
            switch(typeof value) {
                case 'string':
                    return quote(value);
                case 'number':
                    return isFinite(value)?String(value):'null';
                case 'boolean':
                case 'null':
                    return String(value);
                case 'object':
                    if(!value) {
                        return 'null'
                    }
                    partial=[];
                    if(isArray(value)) {
                        length=value.length;
                        for(i=0;i<length;i+=1) {
                            partial[i]=str(i,value)||'null'
                        }
                        v=partial.length===0?'[]':'['+partial.join(',')+']';
                        return v
                    }
                    for(k in value) {
                        if(Object.hasOwnProperty.call(value,k)) {
                            v=str(k,value);
                            if(v) {
                                partial.push(quote(k)+':'+v)
                            }
                        }
                    }
                    v=partial.length===0?'{}':'{'+partial.join(',')+'}';
                    return v
            }
        };
        $.encode=function (value) {
            return str('',{
                '': value
            })
        };
        $.decode=function (text) {
            text=text+"";
            cx.lastIndex=0;
            if(cx.test(text)) {
                text=text.replace(cx,
                function (data) {
                    return '\\u'+('0000'+data.charCodeAt(0).toString(16)).slice(-4)
                })
            }
            return eval('('+text+')')
        }

    })();

    $.ready=(function () {
        var load_events=[],
        load_timer,
        script,
        done,
        exec,
        old_onload,
        init=function () {
            done=true;

            clearInterval(load_timer);

            while(exec=load_events.shift())
                exec();

            if(script) script.onreadystatechange='';
        };

        return function (func) {
            if(done) return func();

            if(!load_events[0]) {
                if(doc.addEventListener)
                    doc.addEventListener("DOMContentLoaded",init,false);

                if($.ie) {
                    (function () {
                        if(done)
                            return;
                        var node=new Image();
                        try {
                            node.doScroll();
                            node=null
                        } catch(e) {
                            setTimeout(arguments.callee,64);
                            return;
                        }
                        init();
                    });
                }
                else if($.webkit) {
                    load_timer=setInterval(function () {
                        if(/loaded|complete/.test(doc.readyState))
                            init();
                    },10);
                }

                old_onload=win.onload;
                win.onload=function () {
                    init();
                    if(old_onload) old_onload();
                };
            }

            load_events.push(func);
        }
    })();

    $.encodeURI=function (o) {
        var res="",v;
        for(var k in o) {
            v=o[k];
            v=isUndefined(v)||v===null?"":encodeURIComponent(v);
            res+=(k+"="+v+"&")
        }
        return res.substr(0,res.length-1)
    };

    var createXHR=win.XMLHttpRequest?
        function () {
            return new XMLHttpRequest()
        } :function () {
            return new ActiveXObject("Microsoft.XMLHTTP")
        };

    var ajaxJson=function (url,callback,method,data,async) {
        async=async!==false;
        callback=callback||function () { };
        var xhr=createXHR(),
        onreadystatechange=function () {
            if(xhr.readyState==4)
                callback(xhr.status==200?$.decode(xhr.responseText):{
                    success: false,
                    msg: xhr.status+"错误",
                    data: xhr.responseText
                });
        };
        if(method=="GET") {
            if(data) url+="?"+$.encodeURI(data);
            data=null;
            xhr.open(method,url,async)
        } else {
            xhr.open("POST",url,async);
            xhr.setRequestHeader("Content-Type","application/x-www-form-urlencoded; charset=utf-8");
            data=data?$.encodeURI(data):null
        }
        if(async) {
            xhr.onreadystatechange=onreadystatechange;
            xhr.send(data)
        } else {
            xhr.send(data);
            onreadystatechange()
        }
    };
    $.get=function (url,callback,data,async) {
        ajaxJson(url,callback,"GET",data,async)
    };
    $.post=function (url,callback,data,async) {
        ajaxJson(url,callback,"POST",data,async)
    };

    $.gradual=function (opts,onstop) {
        var opt;
        for(var i=0;i<opts.length;i++) {
            opt=opts[i];
            opt.times=opt.endTime-opt.startTime+1;
            opt.oriClass=opt.element.className;
            opt.oriStyle=opt.element.style.cssText.replace(/;\s*$/,";");
            opt.isWidth=!isUndefined(opt.widthEnd)&&!isUndefined(opt.widthStart);
            opt.isHeight=!isUndefined(opt.heightEnd)&&!isUndefined(opt.heightStart);
            opt.isLeft=!isUndefined(opt.leftEnd)&&!isUndefined(opt.leftStart);
            opt.isTop=!isUndefined(opt.topEnd)&&!isUndefined(opt.topStart);
            opt.isOpacity=!isUndefined(opt.opacityEnd)&&!isUndefined(opt.opacityStart);
            opt.isEnd=false;
        }

        var counter=0,isStop=false,timer=null;
        var _start=function () {
            isStop=true;
            var cssText;
            for(var i=0;i<opts.length;i++) {
                opt=opts[i];
                cssText="";

                if(!opt.isEnd) {

                    if(counter>=opt.endTime) {
                        if(opt.isWidth) cssText+="width:"+opt.widthEnd+"px;";
                        if(opt.isHeight) cssText+="height:"+opt.heightEnd+"px;";
                        if(opt.isLeft) cssText+="left:"+opt.leftEnd+"px;";
                        if(opt.isTop) cssText+="top:"+opt.topEnd+"px;";
                        if(opt.isOpacity) cssText+="filter:alpha(opacity:"+opt.opacityEnd+");opacity:"+(opt.opacityEnd/100)+";";

                        opt.element.style.cssText=opt.oriStyle+";"+cssText;

                        opt.isEnd=true;

                        if(isFunction(opt.onstop))
                            opt.onstop();

                    } else {
                        isStop=false;

                        if(counter>=opt.startTime) {

                            if(counter==opt.startTime&&isFunction(opt.onstart)) {
                                opt.onstart();
                                opt.oriStyle=opt.element.style.cssText.replace(/;\s*$/,";");
                            }
                            if(opt.isOpacity) {
                                opt.opacityStart+=(opt.opacityEnd-opt.opacityStart)/opt.times;
                                cssText+="filter:alpha(opacity:"+opt.opacityStart+");opacity:"+(opt.opacityStart/100)+";";
                            }
                            if(opt.isTop) {
                                opt.topStart+=(opt.topEnd-opt.topStart)/opt.times;
                                cssText+="top:"+opt.topStart+"px;";
                            }
                            if(opt.isLeft) {
                                opt.leftStart+=(opt.leftEnd-opt.leftStart)/opt.times;
                                cssText+="left:"+opt.leftStart+"px;";
                            }
                            if(opt.isWidth) {
                                opt.widthStart+=(opt.widthEnd-opt.widthStart)/opt.times;
                                cssText+="width:"+opt.widthStart+"px;";
                            }
                            if(opt.isHeight) {
                                opt.heightStart+=(opt.heightEnd-opt.heightStart)/opt.times;
                                cssText+="height:"+opt.heightStart+"px;";
                            }
                            opt.element.style.cssText=opt.oriStyle+";"+cssText;
                            opt.times--;
                        }
                    }

                }
            }

            counter++;
            if(isStop) {
                win.clearInterval(timer);
                if(isFunction(onstop))
                    onstop();
            }
        };
        _start();

        timer=window.setInterval(function () {
            _start();

        },20);

    };

    $.int=function (value) {
        return parseInt(value.replace(/^(\s|0)+/,""));
    };

    $.pad=function (num,n) {
        var a='0000000000000000'+num;
        return a.substr(a.length-n);
    };

    $.Calendar=function () {
        var tmp,args=arguments,reg=/^\d{4}-\d{2}-\d{2}$/,regText="日期格式错误";
        var calendar=this;
        var input=$(args[0]);
        if(tmp=args[1]) {
            tmp.add(input,$.extend(args[2],{
                listen: false,regex: reg,regexText: regText
            }));
        } else
            input.prop("msg",$("SPAN",input.parentNode).css("hid")).prop("validate",function () {
                if(this.value!=""&&!reg.test(this.value))
                    this.msg.css("err").html(regText);
                else
                    this.msg.css("hid")
            });

        var docFrg=doc.createDocumentFragment();

        var container=$("DIV",docFrg)
        .css("calendar");

        var bgFrame=$("IFRAME",container);

        var now=new Date();
        var aDays=[31,28,31,30,31,30,31,31,30,31,30,31];
        var aWeek=["日","一","二","三","四","五","六"];
        var cYear;
        var cMonth;
        var date;

        var content=$("DIV",container)
        .css("clendarCon");

        var top=$("DIV",content)
        .css("clendarTop");

        $("A",top)
        .css("prevMonth")
        .attr("href","javascript:void(0);")
        .on("click",function () {
            var m=$.int(month.value);
            if(m==1) {
                year.value=Math.max(1900,$.int(year.value)-1);
                m=12;
            } else
                m--;
            month.value=$.pad(m,2);
            showCalendar()
        })
        .html("&lt;&lt;");

        $("A",top)
        .attr("href","javascript:void(0);")
        .on("click",function () {
            var m=$.int(month.value);
            if(m==12) {
                year.value=Math.min(now.getFullYear()+10,$.int(year.value)+1);
                m=1;
            } else
                m++;
            month.value=$.pad(m,2);
            showCalendar()
        })
        .css("nextMonth")
        .html("&gt;&gt;");

        var year=$("SELECT",top)
        .on("change",function () {
            if($.int(this.value)%4==0) aDays[1]=29;
            else aDays[1]=28;
            showCalendar()
        });

        for(var i=1900;i<now.getFullYear()+10;i++) {
            year.options.add(new Option(i,i));
        }

        top.appendText("年 ");

        var month=$("SELECT",top)
        .on("change",showCalendar);

        for(var i=1,j;i<=12;i++) {
            j=$.pad(i,2);
            month.options.add(new Option(j,j));
        }
        top.appendText("月");

        var dateCon=$("DIV",content);

        function hideCalendar() {
            container.style.display="none";
            $.Calendar.current=null;
            input.validate();
        };

        function showCalendar() {
            var iMonth=$.int(month.value)-1,
            days=aDays[iMonth];

            container.style.display="block";
            dateCon.innerHTML="";

            var firstDay=new Date($.int(year.value),iMonth,1).getDay();
            var week=$("DIV",dateCon)
            .css("week");

            for(var j=0;j<7;j++) {
                $("SPAN",week)
                .html(aWeek[j]);
            }
            for(var j=0;j<firstDay;j++) {
                $("SPAN",dateCon)
                .html("&nbsp;");
            }
            for(var j=1;j<=days;j++) {

                var a=$("A",dateCon)
                .attr("href","javascript:void(0);")
                .prop("d",$.pad(j,2))
                .html(j)
                .on("click",function () {
                    calendar.value(year.value+"-"+month.value+"-"+this.prop("d"))
                });

                if(j==now.getDate()&&year.value==now.getFullYear()&&$.int(month.value)==now.getMonth()+1)
                    a.className="today";

                if(year.value==cYear&&month.value==cMonth&&date==j)
                    a.className+=" current";
            }
            bgFrame.style.cssText="height:"+content.offsetHeight+"px;width:"+content.offsetWidth+"px;"
        };
        var bottom=$("DIV",content)
        .css("clendarBtm");

        $("A",bottom)
        .css("prevYear")
        .attr("href","javascript:void(0);")
        .html("上年")
        .on("click",function () {
            year.value=Math.max(1900,$.int(year.value)-1);
            showCalendar()
        });

        $("A",bottom)
        .css("nextYear")
        .attr("href","javascript:void(0);")
        .html("下年")
        .on("click",function () {
            year.value=Math.min(now.getFullYear()+10,$.int(year.value)+1);
            showCalendar()
        });

        $("A",bottom)
        .attr("href","javascript:void(0);")
        .html("[今天]")
        .on("click",function () {
            calendar.today()
        });

        $("A",bottom)
        .attr("href","javascript:void(0);")
        .html("[清空]")
        .on("click",function () {
            calendar.value("")
        });

        input.on("focus",function () {
            if(this.value) {
                var aInputDate=this.value.split("-");
                cYear=aInputDate[0];
                cMonth=aInputDate[1];
                date=aInputDate[2]
            } else {
                now=new Date();
                cYear=now.getFullYear();
                cMonth=$.pad(now.getMonth()+1,2);
                date=0;
            }

            year.value=cYear;
            month.value=cMonth;

            if($.Calendar.current)
                $.Calendar.current.hide();
            $.Calendar.current=calendar;
            var tOnClick=doc.body.onclick;
            doc.body.onclick=function (e) {
                e=$E(e);
                var target=e.target;
                if(target!=input&&!$(container).contains(target)) {
                    doc.body.onclick=tOnClick;
                    hideCalendar()
                }
            };

            showCalendar();
        });

        input.parentNode.insertBefore(docFrg,input);
        input.parentNode.style.cssText="overflow:visible;";
        container.style.cssText="margin-top:"+(input.offsetTop+input.offsetHeight)+"px";

        this.hide=hideCalendar;

        this.today=function () {
            now=new Date();
            calendar.value(now.getFullYear()+"-"+$.pad(now.getMonth()+1,2)+"-"+$.pad(now.getDate(),2))
        };

        this.value=function (value) {
            if(isUndefined(value))
                return input.value;

            input.value=value;
            hideCalendar()
        };
    };

    $.move=function (o,c,ex) {
        var mouseX,mouseY,top,left;

        function move(e) {
            c.css({ left: (left+e.pageX-mouseX)+"px",top: (top+e.pageY-mouseY)+"px" });
        };

        if(!c)
            c=o;

        o.on("mousedown",function (e) {
            if((isArray(ex)&&ex.contains(e.target))||(ex&&e.target==ex))
                return;

            var cs=$(c).getCurrentStyle();
            mouseX=e.pageX;
            mouseY=e.pageY;
            top=parseInt(cs.top.replace("px",""));
            left=parseInt(cs.left.replace("px",""));

            var bg=$.move.bg;
            var bgDIV=$.move.bgDIV;
            if(!bg) {
                $.move.bg=bg=$("IFRAME",doc.body)
                .attr("frameborder","0")
                .attr("scrolling","no");
                $.move.bgDIV=bgDIV=$("DIV",doc.body);
            } else {
                bg.appendTo(doc.body);
                bgDIV.appendTo(doc.body);
            }
            var rect=$.getDocRect();
            var css={ display: "block",cursor: "move",zIndex: "10000",top: "0px",left: "0px",position: "absolute",width: rect.width-20+"px",height: rect.height-20+"px",opacity: "0",backgroundColor: "#000" };

            function removeMove() {
                bg.css("display:none");
                bgDIV.css("display:none");
                bgDIV.remove("mouseup",removeMove);
                bgDIV.remove("mousemove",move);
            };

            bg.css(css);
            bgDIV.css(css).on("mouseup",removeMove).on("mousemove",move);
        });
    };

    $.Dialog=function (config) {

        var docElem=doc.documentElement,docBody=doc.body;
        var dialogWidth=(config.width||400)+2,dialogHeight,dialogLeft,dialogTop,centerX,centerY;
        var scrollLeft,scrollTop;

        if(!$.Dialog.bg) {
            $.Dialog.bg=$("IFRAME",doc.body)
                .attr("frameborder","0")
                .attr("scrolling","no")
                .css("dialogBg");
            $.Dialog.bgDIV=$("DIV",doc.body)
                .css("dialogBgDIV");
        }

        var fragment=doc.createDocumentFragment();

        var container=$("DIV",fragment).css("dialog");
        var inside=$("DIV",container);
        var header=$("DIV",inside).css("dialogHeader");
        var closer=$("A",header).css("dialogCloser");
        var title=$("SPAN",header).html(config.title);

        $.move(header,container,closer);

        header.onselectstart=function () { return false; }

        var content=$("DIV",inside).css("dialogCon");

        if(isString(config.content))
            content.html(config.content);
        else
            content.appendChild(config.content);

        doc.body.appendChild(fragment);

        this.close=function () {

            if(isFunction(config.beforeClose)&&config.beforeClose()===false)
                return false;

            $.Dialog.bg.style.display="none";
            $.Dialog.bgDIV.style.display="none";
            container.style.overflow="hidden";

            getRect();

            var dialogOpt={
                element: container,
                startTime: 5,
                endTime: 10,
                topStart: dialogTop,
                topEnd: centerY,
                leftStart: dialogLeft,
                leftEnd: centerX,
                widthStart: container.offsetWidth-2,
                widthEnd: 0,
                heightStart: container.offsetHeight-2,
                heightEnd: 0
            };

            var contentOpt={
                element: inside,
                startTime: 0,
                endTime: 5,
                opacityStart: 100,
                opacityEnd: 0
            };

            $.gradual([dialogOpt,contentOpt],function () {
                container.style.cssText="left:-10000px;top:-10000px;visibility:hidden;height:auto;";
                inside.style.visibility="hidden";

                if(isFunction(config.close))
                    config.close()
            });
        };
        closer.onclick=this.close;

        this.distroy=function () {
            if(container.parentNode)
                container.remove();
        };

        this.title=function (v) {
            title.html(v);
        };

        this.width=function (v) {
            dialogWidth=v+2;
        };

        function getRect() {
            clientWidth=docElem.clientWidth;
            clientHeight=docElem.clientHeight;
            scrollLeft=Math.max(docElem.scrollLeft,docBody.scrollLeft);
            scrollTop=Math.max(docElem.scrollTop,docBody.scrollTop);
            centerX=Math.round(clientWidth/2)+scrollLeft;
            centerY=Math.round(clientHeight/2)+scrollTop;

            dialogHeight=container.offsetHeight;

            dialogLeft=Math.max(Math.round((clientWidth-dialogWidth)/2)+scrollLeft,0);
            dialogTop=Math.max(Math.round((clientHeight-dialogHeight)/2)+scrollTop,0);

        };

        function fixPlace() {
            getRect();
            container.style.left=dialogLeft+"px";
            container.style.top=dialogTop+"px";
        };

        this.fixPlace=fixPlace;
        this.show=function () {
            container.style.overflow="hidden";
            getRect();
            container.style.visibility="visible";

            var height=Math.max(docElem.scrollHeight,docBody.scrollHeight,clientHeight,dialogHeight),
                width=Math.max(docElem.scrollWidth,docBody.scrollWidth,clientWidth);

            var dialogOpt={
                element: container,
                startTime: 0,
                endTime: 5,
                topStart: centerY,
                topEnd: dialogTop,
                leftStart: centerX,
                leftEnd: dialogLeft,
                widthStart: 0,
                widthEnd: dialogWidth-2,
                heightStart: 0,
                heightEnd: dialogHeight-2
            };

            inside.style.cssText="height:100%;filter:alpha(opacity:0);opacity:0;visibility:hidden;";
            var contentOpt={
                element: inside,
                startTime: 5,
                endTime: 15,
                opacityStart: 0,
                opacityEnd: 100,
                onstart: function () {
                    inside.style.visibility="visible";
                }
            };

            $.gradual([contentOpt,dialogOpt],function () {
                container.css({ height: "auto",overflow: "visible" });
                $.Dialog.bg.style.cssText="width:"+width+"px;height:"+height+"px;display:block;";
                $.Dialog.bgDIV.style.cssText="width:"+width+"px;height:"+height+"px;display:block;filter:alpha(opacity:30);opacity:0.3;";

                if(isFunction(config.show))
                    config.show()
            });
        };

    };

    $.page=function (page,pageSize,total,container,jump,more) {
        var totalPages=Math.ceil(total/pageSize);

        container.innerHTML="";

        var info=$("SPAN",container)
        .css({ "float": "left" });

        var moreInfo;
        if(more) {
            moreInfo=$("SPAN",container).css({ "float": "left" });
            if(isString(more))
                moreInfo.html(more);
            else if(isFunction(more))
                more(moreInfo);
        } else
            moreInfo=container;

        var content=$("SPAN",container)
        .css({ "float": "right" });

        if(totalPages>0) {
            function addButton(text,index) {
                $("A",content)
                .attr("href","javascript:void(0);")
                .prop("index",index)
                .html(text)
                .on("click",function () {
                    jump(index);
                    return false
                });
            };
            function addButtons(startPage,endPage) {
                for(var i=startPage;i<=endPage;i++) {
                    if(i==page)
                        $("SPAN",content)
                        .css("current")
                        .html(i);
                    else addButton(i,i)
                }
            };
            if(page==1) $("SPAN",content).html("第一页");
            else addButton("第一页",1);
            if(totalPages<=7) {
                addButtons(1,totalPages)
            } else {
                if(page<5) {
                    addButtons(1,5);
                    content.appendText("...")
                } else {
                    addButtons(1,2);
                    content.appendText("...");
                    if(page+3>=totalPages) {
                        addButtons(totalPages-4,totalPages-1)
                    } else {
                        addButtons(page-1,page+2);
                        content.appendText("...")
                    }
                }
                addButtons(totalPages,totalPages)
            }
            if(totalPages==page) $("SPAN",content).html("最后一页");
            else addButton("最后一页",totalPages);

            $("SPAN",info).html("共"+total+"条数据&nbsp;");
            $("SPAN",info).html("当前第"+page+"/"+totalPages+"页");

            var INPUT=$("INPUT",content)
            .attr("value",page)
            .css("pageNum");

            $("EM",$("A",content)
            .css("btn1")
            .on("click",function () {
                if(isNaN(INPUT.value)||$.int(INPUT.value)>totalPages) alert("请输入正确的页码!");
                else jump($.int(INPUT.value))
            })).html("go");
        }
        else
            $("SPAN",info).html("共0条数据");

        return moreInfo;
    };

    $.Validation=function () {
        var validations=[];
        var validationErrors=[];

        function addError(o,msg) {
            o.msg.css("err").html(msg);
            validationErrors.push(o);
        };

        function removeError(o) {
            if(!isUndefined(o.msg))
                o.msg.css("hid");

            for(var i=validationErrors.length-1;i>=0;i--) {
                if(o===validationErrors[i])
                    validationErrors.splice(i,1);
            }
        };

        this.hasError=function () {
            return validationErrors.length!=0;
        };

        this.validate=function () {
            for(var i=0;i<validations.length;i++) {
                validations[i]();
            }
            return !this.hasError();
        };

        this.add=function (o,opt) {

            if(isString(o))
                o=$(o);

            if(isUndefined(o.msg)) {
                o.msg=$("SPAN",true).appendTo(o.parentNode||o[o.length-1].parentNode).css("hid");
                if(opt.width)
                    o.msg.css({ width: opt.width+"px" });
            }
            else
                $(o.msg);

            var validate=function () {
                var v=isFunction(o.value)?o.value():o.value;

                var flag=false;
                if(opt.emptyAble===false&&v=="")
                    addError(o,opt.emptyText);
                else if(v!=""&&!isUndefined(opt.regex)&&!opt.regex.test(v))
                    addError(o,opt.regexText);
                else if(opt.compare&&opt.compare.value!=v)
                    addError(o,opt.compareText);
                else if(opt.validate&&!opt.validate())
                    addError(o,opt.validationText);
                else {
                    flag=true;
                    removeError(o);
                }
                return flag;
            };
            o.validate=validate;
            if(opt.listen!==false)
                o.on("blur",function () {
                    if(validate()&&isFunction(opt.success))
                        opt.success();
                });

            if(opt.msg)
                o.on("focus",function () {
                    o.msg.css("msg").html(opt.msg);
                });

            validations.push(validate);
        };

    }

})(window,document);
