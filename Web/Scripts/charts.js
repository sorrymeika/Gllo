(function (doc,undefined) {
    var win=this,
		uuuid= -1,
		hasSVG=win.SVGAngle||doc.implementation.hasFeature("http://www.w3.org/TR/SVG11/feature#BasicStructure","1.1"),
		isIE=/msie/i.test(navigator.userAgent)&&!win.opera,
		path=hasSVG?'d':'path',
		seal=hasSVG?'z':'e',
		math=Math,
		mathRound=math.round,
		mathFloor=math.floor,
		mathCeil=math.ceil,
		mathMax=math.max,
		mathMin=math.min,
		mathAbs=math.abs,
		mathCos=math.cos,
		mathSin=math.sin,
		M='M',
		L='L';

    win.extend=function () {
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
                    if(deep&&copy&&typeof copy==="object"&&!copy.nodeType) {
                        target[name]=arguments.callee(deep,src||(copy.length!=null?[]:{}),copy);
                    }
                    else if(copy!==undefined)
                        target[name]=copy;
                }

        }
        return target;
    };

    win.each=function (object,callback,args) {
        var name,i=0,length=object.length;
        if(args) {
            args=Array.prototype.slice.call(arguments).slice(2);
            if(length===undefined) {
                for(name in object)
                    if(callback.apply(object[name],[name,object[name]].concat(args))===false)
                        break;
            } else
                for(;i<length;i++)
                    if(callback.apply(object[i],[i,object[i]].concat(args))===false)   //
                        break;
        } else {
            if(length===undefined) {
                for(name in object)
                    if(callback.call(object[name],name,object[name])===false)
                        break;
            } else
                for(var value=object[0];
					i<length&&callback.call(value,i,value)!==false;value=object[++i]) { }
        }
        return object;
    };

    win.contains=function (p,c) {
        if(!p||!c) return false;
        if(p===c) return true;
        return isIE
			?p.contains(c)
			:p.compareDocumentPosition(c)==20
				?true
				:false;
    };
    //---------------------------------------------------------------
    function processPoint(x) {
        return isIE?~ ~x.toFixed(0):~ ~x.toFixed(0)+0.5;
    };
    function calTextLen(txt,cssStr) {
        var span=doc.createElement('span');
        if(cssStr) {
            typeof cssStr==='string'
				?span.style.cssText=cssStr
				:extend(span.style,cssStr);
        } else {
            extend(span.style,{
                fontSiz: '12px',
                fontFamily: '"Lucida Grande", "Lucida Sans Unicode", Verdana, Arial, Helvetica, sans-serif'
            });
        }
        span.innerHTML=txt||'';
        span.style.visibility='hidden';
        doc.body.appendChild(span);
        var width=span.offsetWidth,
			height=span.offsetHeight;
        doc.body.removeChild(span);
        return { w: width,h: height };
    };
    function angle(r,center,o,jingdu) {
        var hudu=Math.PI*2*(o/360),
			x=center[0]+r*Math.sin(hudu),
			y=center[1]+ -r*Math.cos(hudu);
        return [x.toFixed(jingdu||0),y.toFixed(jingdu||0)];
    }

    function xx(a,b,lineNum) {
        a=Math.min(a,0);
        aa=Math.abs(a);
        var m=Math.max(aa,b),v;

        if(m==0) {
            a=0;
            b=10;
            v=10;
            lineNum=1;
        } else {
            mm=(m+"").match(/^(\d+)(\.\d+){0,1}$/)[1];
            v=Math.pow(10,mm.length-1);
            var d=0,lineNum=0;
            while(d<b) {
                lineNum++;
                d+=v;
            }
            d=0;
            while(d<aa) {
                lineNum++;
                d+=v;
            }
            if(lineNum<5) {
                lineNum=lineNum*2;
                v=v/2;
            } else if(lineNum>=8) {
                lineNum=Math.round(lineNum/2);
                v=v*2;
            }
        }
        return { min: a,max: b,stf: v,lineNum: lineNum };
    }
    //---------------------------------------------------------------------------------------------------------------
    //对svg vml元素的一些创建 修改属性 样式 删除 ==  一些的操作
    win.vector=function () { };
    vector.prototype={
        $c: function (graphic,nodeName) {
            this.element=this[0]=doc.createElementNS('http://www.w3.org/2000/svg',nodeName);
            this.graphic=graphic;
            return this;
        },
        attr: function (hash,val) {
            var elem=this.element,
				key,
				value;
            if(typeof hash==='string') {
                if(val===undefined) {
                    return elem.getAttribute(hash);
                } else {
                    elem.setAttribute(hash,val);
                    return this;
                }
            } else {
                for(key in hash) {
                    value=hash[key];
                    if(key===path) {
                        value&&value.join
							&&(value=value.join(' '));

                        /(NaN|  |^$)/.test(value)
							&&(value='M 0 0');
                    }
                    elem.setAttribute(key,value)
                }
            }
            return this;
        },
        css: function (hash) {
            for(var key in hash) {
                isIE&&key=="opacity"
					?this[0].style['filter']="alpha(opacity="+hash[key]*100+")"
					:this[0].style[key]=hash[key];
            }
            return this;
        },
        on: function (eventName,handler) {
            var self=this;
            /*this.element.addEventListener(eventName,function(){
            handler.call(self)
            },false);*/
            this.element['on'+eventName]=function (e) {
                e=e||win.event;
                handler.call(self,e);
            }
            return this;
        },
        appendTo: function (parent) {
            if(parent) {
                parent.element
					?parent.element.appendChild(this.element)
					:parent.appendChild(this.element)

            } else {
                this.graphic.container.appendChild(this.element);
            }
            return this;
        },
        addText: function (str) {
            var elem=this.element;
            if(elem.nodeName==='text') {
                elem.appendChild(doc.createTextNode(str+''));
            }
            return this;
        },
        setOpacity: function (v) {
            this.attr('fill-opacity',v);
            return this;
        },
        setSize: function (v) {
            this[0].nodeName==='circle'
				?this.attr('r',4+(v===0?0:2))
				:this.attr({ 'stroke-width': v });
            return this;
        },
        toFront: function () {
            this[0].parentNode.appendChild(this[0]);
            return this;
        },
        show: function () {
            this[0].style.display='block';
            return this;
        },
        hide: function () {
            this[0].style.display='none';
            return this;
        },
        destroy: function () {
            //销毁节点......................
            var node=this[0]||this;
            node.onmouseover=node.onmouseout=node.onclick=null;
            node.parentNode
				&&node.parentNode.removeChild(node);
            return this;
        }
    };
    //---------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------
    //如果是vml修改其中的一些方法	
    if(!hasSVG) {
        //-------------创建vml环境-----------------	
        doc.createStyleSheet().addRule(".vml","behavior:url(#default#VML);display:inline-block;position:absolute;left:0px;top:0px");
        !doc.namespaces.vml&&! +"\v1";
        doc.namespaces.add("vml","urn:schemas-microsoft-com:vml");

        //-------------修改一些方法-----------------
        extend(vector.prototype,{
            $c: function (graphic,nodeName) {
                var name=nodeName||'shape';
                this.element=this[0]=(name==='div'||name==='span')
					?doc.createElement(name)
					:doc.createElement('<vml:'+name+' class="vml">');
                this.graphic=graphic;
                return this;
            },
            /*on : function(eventName, handler){
            var self = this;
            this.element.attachEvent("on" + eventName,function(){
            handler.call(self);
            });
            return this;
            },*/
            addText: function (txt) {
                this[0].innerHTML=txt||'';
                return this;
            },
            setSize: function (v) {
                this[0].strokeWeight=v;
                return this;
            },
            setOpacity: function (v) {
                this.opacity.opacity=v;
                return this;
            }
        });
    }
    //---------------------------------------------------------------------------------------------------






    //画图类 
    //------------------------------------------------------------
    win.smipleChart=function () {
        this.init.apply(this,arguments);
    };
    smipleChart.list=[];
    smipleChart.timer=null;
    smipleChart.prototype={
        options: {
            charts: {
                paddingRight: 20,
                radius: 200,
                style: {
                    fontFamily: '"Lucida Grande", "Lucida Sans Unicode", Verdana, Arial, Helvetica, sans-serif',
                    fontSize: '12px',
                    background: '#FFFFFF'
                }
            },
            title: {
                text: '',
                y: 10,
                style: {
                    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
                    fontSize: '16px',
                    fontWeight: 'bold'
                }
            },
            subTitle: {
                text: '',
                y: 30,
                style: {
                    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
                    fontSize: '12px',
                    color: '#111'
                }
            },
            yUnit: {
                text: '',
                style: {
                    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
                    fontSize: '12px',
                    color: '#111'
                },
                lineNum: 10
            }
        },
        init: function (container,options) {
            clearTimeout(smipleChart.timer)
            var self=this;
            this.width=container.offsetWidth;
            this.height=container.offsetHeight;
            this.currList={};
            this.uuuid= ++uuuid;
            this.timer=null;
            //主要画图组的集合 形式
            //{id : {dom:xx,show:true}}
            this.mainGroup={};
            //分段的时候要用到的  知道哪些是隐藏了的  因为要涉及到重绘
            this.hideList={};

            //svg 里面画图 必须有一个svg标签 vml就用div了
            this.container=hasSVG
				?new vector().$c(1,'svg')
					.attr({
					    xmlns: 'http://www.w3.org/2000/svg',
					    version: '1.1'
					})
					.css({ fontSize: '12px' })
					.appendTo(container)
				:new vector().$c(1,'div')
					.css({
					    fontSize: '12px',
					    width: this.width+'px',
					    height: this.height+'px'
					})
					.appendTo(container);


            var c=extend(true,{},this.options),
				opts=this.opts=extend(true,c,options),
				style=extend(opts.charts.style,{
				    width: this.width,
				    height: this.height
				});

            this.loadMe(this);
        },
        loadMe: function () {
            var opts=this.opts,
				self=this,
				type=opts.charts.type;

            this.container=this.container
				.on('mouseout',function (e) {
				    var elem=e.relatedTarget||e.toElement;
				    if(!contains(this[0],elem)) {
				        self.hideTooltip();
				        self.currList.dot
							&&self.currList.dot.setSize(0);
				        self.currList.line
							&&self.currList.line.setSize(1.5);
				        self.currList={};
				    }
				})
				.css({ display: 'none' })[0];


            //计算绘画盘子的时候需要的一些参数
            this.getDrawArea()
				.createTooltip()         //创建提示信息的框框
				.drawTitle()             //画标题
            //画盘子

            'line,area,pie'.indexOf(type)>=0
				&&(opts.charts.panel='x');


            ' pie,pies'.indexOf(type)<0
				&&this.drawPanel();


            this.drawLegend(opts.legend.type);  //画色块条

            var type={
                line: 'drawLine',
                area: 'drawArea',
                columns: 'drawColumns',
                pie: 'drawPie',
                pies: 'drawPies',
                segment: 'drawSegment'
            }[opts.charts.type];
            //开始画图..............
            this[type]();

            this.container.style.display='';

            if(typeof opts.success=="function")
                opts.success()
        },
        createElement: function (nodeName) {
            return new vector().$c(this,nodeName);
        },
        group: function (name) {
            return this.createElement(hasSVG?'g':'div').attr('mark',name);
        },
        getDrawArea: function () {
            var leftWidth=80,
                bottomSize=60,
                opts=this.opts,
				width=this.width,
				height=this.height,
				title=opts.title,
				subTitle=opts.subTitle,
				area={
				    // 去掉坐标轴左边的刻度文本宽度(预估) 80为定值 左边只留80的间距
				    areaWidth: width-leftWidth,
				    // 去掉坐标轴底下的文本和标线的高度
				    areaHeight: height-bottomSize,
				    //原点的X位置  下面会计算到
				    startX: 0,
				    //原点的Y位置  下面会计算到
				    startY: 0,
				    //中心的x坐标 画饼图的时候需要知道圆心的位置
				    centerX: 0,
				    //中心的y坐标 画饼图的时候需要知道圆心的位置
				    centerY: 0
				};

            //如果主标题存在 减去主标题的高度 否则 减去10的高
            area.areaHeight-=(title.text!=='')
				?title.y
				:10;

            // 去掉副标题高度
            area.areaHeight-=(subTitle.text!=='')
				?subTitle.y
				:10

            area.startX=leftWidth;
            area.startY=height-bottomSize;

            //圆心的位置
            area.centerX=width/2;
            area.centerY=height/2;

            //右边留一些空隙
            area.areaWidth-=30;
            //上边也留一些间距
            area.areaHeight-=15;

            opts.area=area;

            return this;

        },
        drawTitle: function () {
            var opts=this.opts,
				self=this,
				arr=[opts.title,opts.subTitle,opts.yUnit],
            //3个标题坐标的位置的基本参数
				config=[
					{
					    x: this.width/2,
					    y: opts.title.y
					},
					{
					    x: this.width/2,
					    y: opts.subTitle.y
					},
					{
					    x: opts.yUnit.x,
					    y: this.height/2-20
					}
				],
				tpanel=this.group('title')
					.appendTo();

            each(arr,function (i,title) {
                var text=title.text;
                if(text) {
                    var elem=self.baseDraw.span(self,{
                        'text-anchor': 'left',
                        x: mathMax(config[i].x-calTextLen(text,title.style).w/2,10),
                        y: config[i].y
                    },calTextLen(title.text,title.style).h)
						.css(title.style)
						.addText(text)
						.appendTo(tpanel);

                    //如果为2的时候 就说明是副标题  将它竖过来
                    if(i===2) {
                        hasSVG
							?elem.attr({ transform: 'rotate(270, '+(opts.yUnit.x+10)+', '+self.height/2+')' })
							:(elem.element.style.filter='progid:DXImageTransform.Microsoft.BasicImage(rotation=3)')
                    }
                }
            });
            return this;
        },


        //画盘子  比较麻烦
        drawPanel: function (type) {
            var opts=this.opts,
				self=this,
				area=opts.area,
				chartsType=opts.charts.type,
				isSegment=chartsType==='segment',
            //盘子的类型 是横盘子 还是纵盘子
				type=opts.charts.panel||'x';

            // 底板
            var drawAreaWidth=area.areaWidth,
				drawAreaHeight=area.areaHeight,
            //原点的坐标
				startX=area.startX,
				startY=area.startY;

            var allData=[],
				minValue=0,
				maxValue=10,
            //线的条数 只能在1到10之间
				lineNum=mathMin(10,mathMax(opts.yUnit.lineNum,1)),
				staff;

            //组合所有的数据
            each(opts.chartData,function (i,o) {
                // 如果是柱状图 是对所有的数据求和
                isSegment
					?each(o.data,function (j,d) {
					    allData[j]
							?allData[j]=allData[j]+(~ ~d)
							:allData[j]= ~ ~d;
					})
					:allData=allData.concat(o.data)
            });

            //给所有的数据排序  为了下面求最大值 最小值
            allData.sort(function (a,b) { return a-b });

            //求出最大值 最小值
            maxValue=allData[allData.length-1];

            each(allData,function (i,o) {
                if(o!==null) {
                    minValue=o;
                    return false;
                }
            });

            //主盘子容器
            var panel=this.group('panel').appendTo();

            var result=xx(minValue,maxValue,lineNum),
				min=result.min,
				max=result.max,
				f=result.stf,
                lineNum=result.lineNum;

            isSegment
				&&(min=0);
            //表示画的是横坐标 或者是双坐标
            if(type.toLowerCase()==='x') {
                //横坐标单位间隔
                var xPices=drawAreaWidth/opts.xUnit.units.length,
                //单位间隔的中心点
					offset=xPices/2,

					yPices=drawAreaHeight/lineNum;


                //--------------------------------画横向的点和文字---------------------------------------------------------
                var y=hasSVG?5:10,
					t=1000,
					span;
                each(opts.xUnit.units,function (i,d) {
                    self.baseDraw.path(self,{
                        border: 1,
                        borderColor: '#C0C0C0',
                        isfill: false,
                        path: [
							M,
							processPoint(startX+(i*xPices)+(xPices/2)),
							processPoint(startY),
							L,
							processPoint(startX+(i*xPices)+(xPices/2)),
							processPoint(startY+5)
						]
                    }).
					appendTo(panel);

                    span=self.baseDraw.span(self,{
                        x: startX+offset+(i*xPices),
                        y: startY+y,
                        'text-anchor': 'middle'
                    })
						.css({
						    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
						    fontSize: '12px'
						})
						.addText(opts.xUnit.units[i])
						.appendTo(panel)[0];


                    !hasSVG
						&&(span.style.left=parseInt(span.style.left)-span.offsetWidth/2+'px');

                });
                //--------------------------------画纵向的点和文字-----------------------------------------------------------------------

                self.baseDraw.path(self,{
                    border: 1,
                    borderColor: '#c0c0c0',
                    isfill: false,
                    path: [
                            M,
                            startX,
                            processPoint(startY),
                            L,
                            processPoint(startX),
                            processPoint(startY-drawAreaHeight-5)
                        ]
                }).css({ zIndex: -10 })
				.appendTo(panel);

                for(i=0;i<=lineNum;i++) {
                    self.baseDraw.path(self,{
                        border: 1,
                        borderColor: '#c0c0c0',
                        isfill: false,
                        path: [
                            M,
                            startX-5,
                            processPoint(startY-(i*yPices)),
                            L,
                            processPoint(startX),
                            processPoint(startY-(i*yPices))
                        ]
                    }).css({ zIndex: -10 })
					.appendTo(panel);

                    self.baseDraw.path(self,{
                        border: 1,
                        borderColor: i!==0?"#ddd":'#c0c0c0',
                        isfill: false,
                        path: [M,startX,processPoint(startY-(i*yPices)),L,processPoint(startX+drawAreaWidth),processPoint(startY-(i*yPices))]
                    })
					.css({ zIndex: -10 })
					.appendTo(panel);

                    var span=self.baseDraw.span(self,{
                        x: startX-25,
                        y: startY-i*yPices-calTextLen(min+i*f+'').h/2,
                        'text-anchor': 'middle'
                    })
					.css({
					    'font-family': 'Verdana,Arial,Helvetica,sans-serif',
					    'font-size': '12px',
					    width: '40px',
					    display: 'block',
					    textAlign: 'right'
					})
					.addText((min*t+(i*t*f/t)*t)/t+'')
					.appendTo(panel)[0];

                    if(!hasSVG) {
                        span.style.top=parseInt(span.style.top)+span.offsetHeight/2-5+'px';
                        span.style.left=parseInt(span.style.left)-35+'px'
                    }
                }

            } else {
                //横坐标单位间隔
                var yPices=drawAreaHeight/(opts.xUnit.units.length),
                //单位间隔的中心点
					offset=Math.round(yPices/2),
					x=hasSVG?25:70,
					y=hasSVG?0:5,
					span

                each(opts.xUnit.units,function (i,d) {
                    self.baseDraw.path(self,{
                        border: 1,
                        borderColor: '#C0C0C0',
                        isfill: false,
                        path: [
							M,
							processPoint(startX-5),
							processPoint(startY-i*yPices),
							L,
							processPoint(startX),
							processPoint(startY-i*yPices),
						]
                    })
					.appendTo(panel);
                    span=self.baseDraw.span(self,{
                        x: startX-x,
                        y: startY-i*yPices-offset-calTextLen(d).h/2+y,
                        'text-anchor': 'middle'
                    })
					.css({
					    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
					    fontSize: '12px',
					    width: '60px',
					    textAlign: 'right'
					})
					.addText(d)
					.appendTo(panel)

                });

                var xPices=drawAreaWidth/lineNum;

                for(var i=0;i<=lineNum;i++) {
                    self.baseDraw.path(self,{
                        border: 1,
                        borderColor: '#C0C0C0',
                        isfill: false,
                        path: [
							M,
							processPoint(startX+(i*xPices)),
							processPoint(startY),
							L,
							processPoint(startX+(i*xPices)),
							processPoint(startY-drawAreaHeight)
						]
                    }).
					appendTo(panel);

                    self.baseDraw.span(self,{
                        x: startX-calTextLen(min+i*f+'').w/2+i*xPices,
                        y: startY,
                        'text-anchor': 'left'
                    })
					.css({
					    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
					    fontSize: '12px'
					})
					.addText(min+i*f+'')
					.appendTo(panel);
                }

            }

            //-----------------------------------------------------------------------------------------------------	
            //因为起点很可能不是从0开始的  所以在起点的时候要要加上到0那部分的值

            var jianju=0;
            if(min>0) jianju=min;
            if(max<0) jianju=max;

            startX=opts.charts.panel==='x'?startX:startX-xPices*(min/f);

            startY=opts.charts.panel==='x'?startY+yPices*(min/f):startY;

            opts.draw={
                startX: startX,  // X 轴起点
                startY: startY,  // Y 轴起点
                xPices: xPices,  // X 轴每份的宽度
                yPices: yPices,  // Y 轴每份的宽度
                offset: offset,  // X 单分中心点位置偏移量
                jianjuY: jianju*yPices/f,
                jianjuX: jianju*xPices/f,
                feed: f		  // Y 轴的每份有多少	
            }
            return this;
        },

        createTooltip: function () {
            //一个组
            this.tipC=this.group('tip')
				.css({ zIndex: 200,height: '20px',width: '20px',position: 'absolute' })
				.appendTo()
				.hide()

            //画一个框框baseDraw	
            this.tipBox=this.baseDraw.rect(this,{ arc: 0.22,fill: '#fff',border: 2,borderColor: '#606060' })
				.appendTo(this.tipC)

            //因为svg里面的g可以直接定位 但是vml里面的group渲染很慢 所以改div  所以这里的父不一洋
            var p=isIE?this.tipBox:this.tipC;

            this.tipTxtContainer=this.baseDraw.text(this,{ fill: '#000000',x: 5,y: 19,'text-anchor': 'left' })
				.css({
				    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
				    fontSize: '12px',
				    background: '#FFF'
				})
				.appendTo(p);

            this.tipText=doc.createTextNode('');
            this.tipTxtContainer[0].appendChild(this.tipText);
            return this;
        },
        showTooltip: function (obj,x,y,data) {

            /*var txt  = obj.name +':' + data,
            size = calTextLen(txt,this.tipTxtContainer[0].style.cssText),
            pos  = {x : x - (size.w + 5 * 2)/2 ,y : y - 32};
            this.tipC
            .toFront()
            .show();

            if(hasSVG){
            this.tipC.attr({transform:'translate('+pos.x+','+pos.y+')'});
				
            this.tipBox
            .attr({width  : size.w + 5 * 2,height : size.h + 5 * 2,stroke : obj.color||'#606060'});
            }else{
            this.tipC.css({left:pos.x,top:pos.y});
				
            this.tipBox
            .css({width:size.w + 5 * 2,height : size.h + 5 * 2})
            this.tipBox[0].strokeColor = obj.color||'#000';				
            }

            this.tipText.nodeValue = txt || '';*/
            clearTimeout(this.timer);
            var txt=obj.name+':'+data,
				self=this,
				size=calTextLen(txt,this.tipTxtContainer[0].style.cssText),
				pos={ x: x-(size.w+5*2)/2,y: y-32 };

            if(hasSVG) {
                self.tipBox
					.attr({ width: size.w+5*2,height: size.h+5*2,stroke: obj.color||'#606060' });
            } else {
                self.tipBox
					.css({ width: size.w+5*2,height: size.h+5*2 })
                self.tipBox[0].strokeColor=obj.color||'#000';
            }

            this.tipText.nodeValue=txt||'';

            if(this.tipC[0].style.display==='none') {

                hasSVG
					?self.tipC.attr({ transform: 'translate('+pos.x+','+pos.y+')',pos: pos.x+'-'+pos.y })
					:self.tipC.attr({ pos: pos.x+'-'+pos.y }).css({ left: pos.x,top: pos.y });

                this.tipC
					.toFront()
					.show();

            } else {
                var move=function (t,b,c,d) {
                    return c*(t/=d)*t+b;
                },
					t=0,
					b=self.tipC.attr('pos').split('-'),
					c=[pos.x,pos.y],
					d=5;

                this.timer=setInterval(function () {
                    if(t<d) {
                        t++;

                        var x=move(t,~ ~b[0],(~ ~c[0])-(~ ~b[0]),d),
						y=move(t,~ ~b[1],(~ ~c[1])-(~ ~b[1]),d);
                        hasSVG
						?self.tipC.attr({ transform: 'translate('+x+','+y+')',pos: x+'-'+y })
						:self.tipC.attr({ pos: x+'-'+y }).css({ left: x,top: y });
                    } else {
                        clearTimeout(self.timer);
                    }
                },1);
            };
        },
        hideTooltip: function () {
            this.tipC.hide();
        },

        drawLegend: function (type,redraw) {
            var self=this,
				opts=this.opts,
				isLine=opts.charts.type==='line',
            //颜色块的大小
				t_width=20,
				t_height=20,
            //块之间的距离
				t_space=5,
				datas=opts.chartData,
				len=datas.length,
				css=opts.legend.style,

            //最大长度 如果是纵着的 需要最大的长度
				maxWidth=10,
				maxHeight=30,
            //这个东西的位置
				orig_pos=opts.legend.pos?opts.legend.pos:[2,2],

            //显示隐藏组的函数
				handle=function (i) {
				    var g=self.mainGroup['chart'+i],
						issegment=opts.charts.type==='segment';

				    if(g.show) {
				        g.chart.hide();
				        g.show=false;
				        hasSVG
							?this.attr({ fill: '#ccc' })
							:this[0].style.color='#ccc';


				        //如果是分段图  是会涉及到重画的
				        if(issegment) {
				            self.hideList[i]='';
				            var mainGroup=self.mainGroup;

				            for(var name in mainGroup) {
				                var parent=mainGroup[name].chart,
									nodes=parent[0].childNodes,
									len=nodes.length;
				                //销毁图上面画的东西
				                for(var i=len-1;i>=0;i--) {
				                    vector.prototype.destroy.call(nodes[i])
				                }
				            }
				            //重画	
				            self.drawSegment();
				        }

				    } else {
				        g.chart.show();
				        g.show=true;
				        hasSVG
							?this.attr({ fill: '#000' })
							:this[0].style.color='#000'

				        if(issegment) {
				            delete self.hideList[i];
				            var mainGroup=self.mainGroup;

				            for(var name in mainGroup) {

				                var parent=mainGroup[name].chart,
									nodes=parent[0].childNodes,
									len=nodes.length;
				                for(var i=len-1;i>=0;i--) {
				                    vector.prototype.destroy.call(nodes[i])
				                }

				            }
				            self.drawSegment();
				        }
				    }
				},

				arr=[];

            type=type||'lateral';
            var legendPanel=self.group('Legend')
				.appendTo();
            if(type==='lateral') {
                //如果是横着的
                var top=orig_pos[1]+5,
					th=hasSVG?0:3,
					left=orig_pos[0]+5;
                each(datas,function (i,d) {
                    left=i===0?left:t_space+left;
                    //计算所有left的位置

                    //如果是线性图  按线性图的方式画图
                    if(isLine) {
                        self.baseDraw.path(self,{
                            border: 1.5,
                            borderColor: d.color,
                            isfill: false,
                            path: [
								M,
								left.toFixed(0),
								(top+10).toFixed(0),
								L,
								(left+25).toFixed(0),
								(top+10).toFixed(0)
							]
                        })
						  .appendTo(legendPanel);
                        self.baseDraw[d.dotType||'circle'](self,{
                            x: left+12,
                            y: top+10,
                            r: 4,
                            fillColor: d.color
                        })
						.appendTo(legendPanel);
                    } else {
                        self.baseDraw.rect(self,{
                            arc: 0.1,
                            fill: d.color,
                            border: 1,
                            borderColor: d.color,
                            left: left,
                            top: top,
                            width: t_width+'px',
                            height: t_height+'px'
                        })
						.appendTo(legendPanel)
                    }

                    left=left+t_width+2+t_space;
                    var w=calTextLen(d.name,css).w
                    self.baseDraw.span(self,{
                        'text-anchor': 'left',
                        x: left,
                        y: top+th
                    })
					.css(extend(css,{ cursor: 'pointer' }))
					.on('click',function () {
					    handle.call(this,i);
					})
					.addText(d.name)
					.appendTo(legendPanel);
                    left=left+w;

                });
                this.baseDraw.rect(this,{
                    arc: 0.1,
                    fill: 'none',
                    border: 1.5,
                    borderColor: '#666666',
                    width: left+t_space-orig_pos[0],
                    height: maxHeight,
                    left: orig_pos[0],
                    top: orig_pos[1]
                })
					.appendTo(legendPanel);
            } else {

                var top=orig_pos[1]+5,
					th=hasSVG?0:3,
					left=orig_pos[0]+5;
                each(datas,function (i,d) {
                    top=i===0?top:t_space+top;
                    self.baseDraw.rect(self,{
                        arc: 0.1,
                        fill: d.color,
                        border: 1,
                        borderColor: d.color,
                        left: left,
                        top: top,
                        width: t_width+'px',
                        height: t_height+'px'
                    })
					.appendTo(legendPanel);

                    var h=calTextLen(d.name,css).h;

                    self.baseDraw.span(self,{
                        'text-anchor': 'left',
                        x: left+t_width+2+t_space,
                        y: top+th
                    })
					.css(extend(css,{ cursor: 'pointer' }))
					.addText(d.name)
					.on('click',function () {
					    //如果是多层饼图 不行进隐藏				 
					    if(opts.charts.type==='pies') return;
					    handle.call(this,i);
					})
					.appendTo(legendPanel);
                    top=top+h+t_space;
                    maxWidth=Math.max(maxWidth,calTextLen(d.name,css).w);
                });
                this.baseDraw.rect(this,{
                    arc: 0.1,
                    fill: 'none',
                    border: 1.5,
                    borderColor: '#666666',
                    width: maxWidth+22+15,
                    height: top+t_space-orig_pos[1],
                    left: orig_pos[0],
                    top: orig_pos[1]
                })
					.appendTo(legendPanel);
            }
            return this;
        },
        drawLine: function () {
            var self=this,
				opts=this.opts,
				draw=opts.draw;
            each(opts.chartData,function (i,o) {
                var id='chart'+i,
					lineGroup=self.group(id)
						.appendTo();
                self.mainGroup[id]={
                    chart: lineGroup,
                    show: true
                };
                var path=[M],
					data=o.data,
					line;

                for(var i=0,l=data.length;i<l;i++) {
                    if(data[i]==null) {
                        //如果这个数据不存在 并且不是第一个数据 路径上加 M
                        if(path[path.length-1]!==M)
                            path.push(M);
                    } else {
                        //如果不是第一个数据 路径添加L
                        i!==0&&path.push("L");
                        //如果前面一个是null 并且不是第一个  把那个L去掉
                        if(i>0&&data[i-1]==null)
                            path.pop();
                        //计算出 点的x,y的位置	
                        var x=draw.startX+draw.offset+(i*draw.xPices),
							y=draw.startY-data[i]*(draw.yPices/draw.feed);
                        if(isIE) {
                            x=parseInt(x);
                            y=parseInt(y);
                        }
                        path.push(x);
                        path.push(y);

                        //画点
                        var dotType=o.dotType||'circle';

                        self.baseDraw[dotType](self,{
                            x: x,
                            y: y,
                            r: 2,
                            fillColor: o.color
                        })
						.css({ zIndex: 10,cursor: 'pointer' })
                        .appendTo(lineGroup);

                        self.baseDraw[dotType](self,{
                            x: x,
                            y: y,
                            r: 1,
                            fillColor: "Transparent"
                        })
						.css({ zIndex: 100 })
                        .appendTo(lineGroup);

                        self.baseDraw[dotType](self,{
                            x: x,
                            y: y,
                            r: 4,
                            fillColor: "Transparent"
                        })
						.attr({ data: data[i],pos: x+'-'+(y-5) })
						.css({ zIndex: 10,cursor: 'pointer' })
						.on('mouseover',(function (o,x,y) {
						    return function () {
						        if(self.currList.dot) {
						            if(self.currList.dot[0]===this[0])
						                return;
						            self.currList.dot.setSize(0);
						            self.currList.line.setSize(1.5);
						        }
						        var pos=this.attr('pos').split('-');
						        self.showTooltip(o,pos[0],pos[1],this.attr('data'));
						        self.currList.dot=this;
						        self.currList.line=line;
						    }
						})(o,x,y))
                        /*.on('mouseout',function(){				
                        this.setSize(0);
                        line.setSize(1.5);
                        })*/
						.on('click',function () { lineGroup.toFront(); })
						.appendTo(lineGroup);

                    }
                };
                //画折线
                line=self.baseDraw.path(self,{
                    border: 1.5,
                    borderColor: o.color,
                    isfill: false,
                    path: path
                })
				.css({ zIndex: -1 })
                /*.on('mouseover',function(){
                this.setSize(2.5);
                })
                .on('mouseout',function(){										
                this.setSize(1.5);
                })*/
				.on('click',function () { lineGroup.toFront(); })
				.appendTo(lineGroup);
            });
            return this;
        },
        drawArea: function () {
            var self=this,
				opts=this.opts,
				draw=opts.draw;
            each(opts.chartData,function (i,o) {
                var id='chart'+i,
					areaGroup=self.group(id).appendTo();
                self.mainGroup[id]={ chart: areaGroup,show: true };
                //有2个路径 一个是区域的路径 一个是线的路径

                var areaPath=[M,(draw.startX+draw.offset).toFixed(0),(draw.startY-draw.jianjuY).toFixed(0)],
					path=[M],
					data=o.data,
					line;
                for(var n=0,l=data.length;n<l;n++) {
                    //如果数据是空的
                    var len=areaPath.length;
                    if(data[n]===null) {
                        //如果前面的一个不是m 就重新画 所以加上 M
                        if(path[path.length-1]!==M)
                            path.push(M);

                        //如果第1个 或者前面的都为null 修改起点坐标
                        len===3
							&&(areaPath[1]=(draw.startX+(n+1)*draw.xPices+draw.offset).toFixed(0));

                        //如果前面一个不是结束标识符  区域图结束 如果第一个数据是null 则不进行下面的操作
                        if(areaPath[len-1]!==seal&&n!==0) {
                            areaPath=areaPath.concat([
								areaPath[len-2],
								(draw.startY-draw.jianjuY).toFixed(0),
								seal
							]);
                        }
                    } else {
                        n!==0&&path.push(L);
                        areaPath.push(L);
                        //如果前面的那个数据是null 把之前的那个L去掉
                        if(n>0&&data[n-1]==null) {
                            path.pop();
                            //如果是第一个为null 不删除L
                            n!==1&&areaPath.pop();
                        }

                        var x=draw.startX+draw.offset+(n*draw.xPices),
								y=draw.startY-data[n]*(draw.yPices/draw.feed);
                        if(isIE) {
                            x=parseInt(x);
                            y=parseInt(y);
                        }
                        path.push(x);
                        path.push(y);


                        if(areaPath[len-1]===seal) {
                            areaPath=areaPath.concat([
									M,
									x,
									parseInt(draw.startY-draw.jianjuY),
									L,
									x,
									y
								]);
                        } else {
                            areaPath.push(x);
                            areaPath.push(y);
                        }

                        //如果是最后一个点
                        if(n===l-1) {
                            areaPath.push(x);
                            areaPath.push(parseInt(draw.startY-draw.jianjuY));
                        }

                        //画点
                        self.baseDraw[o.dotType||'circle'](self,{
                            x: x,
                            y: y,
                            r: 2,
                            fillColor: o.color
                        })
							.attr({ data: data[n],pos: x+'-'+(y-5) })
							.on('mouseover',(function (o,x,y) {
							    return function () {
							        if(self.currList.dot) {
							            if(self.currList.dot[0]===this[0])
							                return;
							            self.currList.dot.setSize(0);
							            self.currList.line.setSize(1.5);
							        }
							        this.setSize(2);
							        line.setSize(2.5);
							        var pos=this.attr('pos').split('-');
							        self.showTooltip(o,pos[0],pos[1],this.attr('data'));
							        self.currList.dot=this;
							        self.currList.line=line;
							    }

							})(o,x,y))
                        /*.on('mouseout',function(){
                        this.setSize(0);
                        line.setSize(1.5);
                        //self.hideTooltip()
                        })*/
							.on('click',function () { areaGroup.toFront(); })
							.css({ zIndex: 10,cursor: 'pointer' })
							.appendTo(areaGroup);

                    }
                }

                areaPath.push(seal)

                self.baseDraw.path(self,{
                    border: 0,
                    isfill: true,
                    fillColor: o.color,
                    opacity: 0.5,
                    path: areaPath
                })
				.css({ zIndex: 5 })
				.appendTo(areaGroup);

                line=self.baseDraw.path(self,{
                    border: 1.5,
                    borderColor: o.color,
                    isfill: false,
                    path: path
                })
                /*.on('mouseover',function(){
                hasSVG 
                ? this.attr({'stroke-width':2.5})
                : (this[0].strokeWeight = 2.5);
                })
                .on('mouseout',function(){
                hasSVG 
                ? this.attr({'stroke-width':1.5})
                : (this[0].strokeWeight = 1.5);
                })*/
				.on('click',function () { areaGroup.toFront(); })
				.css({ zIndex: -1 })
				.appendTo(areaGroup);
            });
            return this;
        },
        drawColumns: function () {
            var self=this,
				opts=this.opts,
				draw=opts.draw,
				chartData=opts.chartData,
				dataLen=chartData.length,
            //多个柱子之间的间距
				columnSpace=3,
            //一个位置中 所有的间隔之和
				columnPadding=columnSpace*dataLen+columnSpace,
            //每个柱子的宽度
				columnSize=self.opts.charts.panel==='x'
					?Number(((draw.xPices-columnPadding)/dataLen).toFixed(0))
					:Number(((draw.yPices-columnPadding)/dataLen).toFixed(0));

            each(chartData,function (i,o) {

                var data=o.data,
					id='chart'+i,
					isX=opts.charts.panel==='x',
					colGroup=self.group(id).appendTo(),
                //每个点开始的位置
					start=self.opts.charts.panel==='x'
						?draw.startX+columnSpace+i*(columnSize+columnSpace)
						:draw.startY+columnSpace+i*(columnSize+columnSpace)

                self.mainGroup[id]={ chart: colGroup,show: true };

                for(var j=0,l=data.length;j<l;j++) {
                    if(data[j]===null) continue;
                    //如果是横盘子
                    if(isX) {
                        var x=Number((start+j*draw.xPices).toFixed(0)),
							y=Number((draw.startY-draw.jianjuY).toFixed(0)),
							height=Number((data[j]*(draw.yPices/draw.feed)-draw.jianjuY).toFixed(0)),
							path=[
								M,
								x,
								y,
								L,
								x,
								y-height,
								L,
								x+columnSize,
								y-height,
								L,
								x+columnSize,
								y,
								seal
							];
                        var pos=[x+columnSize/2,data[j]>0?y-height:draw.startY-draw.jianjuY];
                    } else {

                        var x=Number((draw.startX+draw.jianjuX).toFixed(0)),
						 	 width=Number((data[j]*((draw.xPices/draw.feed))-draw.jianjuX).toFixed(0)),
						 	 y=Number((start-(j+1)*draw.yPices).toFixed(0)),
							 path=[
								M,
								x,
								y,
								L,
								x+width,
								y,
								L,
								x+width,
								y+columnSize,
								L,
								x,
								y+columnSize,
								seal
							];
                        var pos=[draw.startX+draw.jianjuX+width/2,y];
                    }

                    self.baseDraw.path(self,{
                        border: 0,
                        isfill: true,
                        fillColor: o.color,
                        opacity: 1,
                        path: path
                    })
					.attr({ data: data[j],pos: pos[0]+'-'+pos[1] })
					.css({ zIndex: 5,cursor: 'pointer' })
					.on('mouseover',(function (d) {

					    return function () {
					        this.setOpacity(0.85);
					        var pos=this.attr('pos').split('-')
					        self.showTooltip(o,pos[0],pos[1],this.attr('data'));
					    }

					})(data[j])

					)
					.on('mouseout',function () {
					    this.setOpacity(1);
					})
					.appendTo(colGroup);

                }
            });
            return this;
        },
        drawPie: function () {
            var self=this,
				opts=this.opts,
				area=opts.area,
				rx=area.centerX,
				ry=area.centerY,
				inc=0,
				total=0,
				data=[],
				cumulative= -0.25, // start at top;
				circ=2*Math.PI,
				radiu=mathMin(opts.charts.radius,mathMin(area.areaWidth/2,area.areaHeight/2)),
				fraction,
				half_fraction;

            each(opts.chartData,function (i,o) {
                typeof o.data==='object'
					?(data.push((function (o) {
					    var all=0;
					    for(var i in o)
					        all+= ~ ~o[i]
					    return all
					})(o.data)))
					:data.push(mathAbs(o.data))
            });

            each(data,function (i,o) {
                total=total+o;
            });

            each(data,function (i,o) {
                var pieGroup=self.group('chart'+i).appendTo(),
					s=inc/total*360,
					e=(inc+o)/total*360,
					name=opts.chartData[i].name,
					size=calTextLen(name),
					dot=angle(radiu,[rx,ry],s+(e-s)/2,2),
					x=rx+(dot[0]-rx)/2-size.w/2,
					y=ry+(dot[1]-ry)/2-size.h/2,
					len=Math.sqrt((x-rx)*(x-rx)+(y-ry)*(y-ry)),
					moveDisplacement=((x-rx)*8/len)+','+((y-ry)*8/len);

                inc=inc+o;

                var value=Number(o);
                fraction=total?value/total:0;
                half_fraction=total?(value/2)/total:0;

                var start=cumulative*circ;
                half_cumulative=cumulative+half_fraction;
                cumulative+=fraction;

                var end=cumulative*circ;

                self.baseDraw.pie(self,{
                    config: opts.chartData[i],
                    s: start,
                    e: end,
                    r: radiu,
                    innerR: 0
                })
				.css({ zIndex: 5,cursor: 'pointer' })
				.attr({ move: moveDisplacement,pos: dot[0]+'-'+dot[1] })
				.on('mouseover',function () {
				    this.setOpacity(0.85);
				    var pos=this.attr('pos').split('-');
				    self.showTooltip(opts.chartData[i],pos[0],pos[1],((e-s)/360*100).toFixed(0)+'%')
				})
				.on('mouseout',function (e) {
				    var elem=e.toElement||e.relatedTarget;
				    //如果碰到里面的文本 或者是提示框  不消失
				    if(!elem||contains(this[0].parentNode,elem)||contains(self.tipC[0],elem))
				        return;
				    self.hideTooltip();
				    this.setOpacity(1);
				})
				.on('click',function () {
				    var m=this.attr('move')
				    if(m.indexOf('+')>0) {
				        hasSVG
							?this.attr({
							    transform: 'translate(0,0)'
							})
							:this.css({
							    left: '0px',
							    top: '0px'
							})
				        this.attr({ move: m.replace('+','') });

				    } else {
				        var s=m.split(',');
				        hasSVG
							?this.attr({
							    transform: 'translate('+m+')'
							})
							:this.css({
							    left: s[0]+'px',
							    top: s[1]+'px'
							})

				        this.attr({ move: m+'+' });
				    }
				})
				.appendTo(pieGroup);

                self.mainGroup['chart'+i]={
                    chart: pieGroup,
                    show: true
                };

                self.baseDraw.span(self,{
                    x: x,
                    y: y,
                    fill: '#fff',
                    'text-anchor': 'left'
                })
					.css({
					    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
					    fontSize: '12px',
					    position: 'absolute',
					    color: '#fff',
					    cursor: 'pointer',
					    zIndex: 10
					})
					.addText(name)
					.appendTo(pieGroup);
            });

        },
        drawPies: function () {
            var self=this,
				opts=this.opts,
				area=opts.area,
				rx=area.centerX,
				ry=area.centerY,
				total=0,
				data=[],
				chartData=opts.chartData,
				cumulative= -0.25, // start at top;
				circ=2*Math.PI,
				radiu=mathMin(opts.charts.radius,mathMin(area.areaWidth/2,area.areaHeight/2)),
				fraction,
				half_cumulative,
				half_fraction;

            each(chartData,function (i,o) {
                each(o.data,function (j,d) {
                    data[j]
						?data[j]+=mathAbs(d)
						:data[j]=mathAbs(d)
                });

            });
            //看有多少个数据来生成来生成内半径
            var len=data.length,
				innerSpace=radiu/10;
            Rpice=(radiu-innerSpace)/len;
            each(data,function (i,d) {
                var inc=0;
                if(d===0) return;
                each(chartData,function (j,o) {
                    if(~ ~o.data[i]===0) return;
                    var outR=radiu-Rpice*i,
						innerR=radiu-Rpice*(i+1),
						value= ~ ~o.data[i],
						fraction=value/d;
                    half_fraction=(value/2)/d,
						start=cumulative*circ,
						s=inc/d*360,
						e=(inc+value)/d*360,
						id='chart'+j,
						piesGroup=self.mainGroup[id]?self.mainGroup[id].chart:self.group(id).appendTo();
                    !self.mainGroup[id]
						&&(self.mainGroup[id]={ chart: piesGroup,show: true });
                    inc=inc+value;
                    var name=o.name,
						size=calTextLen(name),
						dot=angle(radiu,[rx,ry],s+(e-s)/2,2),
						showDot=angle(radiu-Rpice*i,[rx,ry],s+(e-s)/2,2),
						px=dot[0]>rx?1:-1,
						py=dot[1]>ry?1:-1;
                    var x=rx+px*innerSpace+((dot[0]-rx-px*innerSpace)/len)*(len-i-1)+((dot[0]-rx-px*innerSpace)/len)/2-size.w/2,
						y=ry+py*innerSpace+((dot[1]-ry-py*innerSpace)/len)*(len-i-1)+((dot[1]-ry-py*innerSpace)/len)/2-size.h/2;

                    half_cumulative=cumulative+half_fraction,
					cumulative+=fraction,
					end=cumulative*circ;

                    self.baseDraw.pie(self,{
                        config: o,
                        s: start,
                        e: end,
                        r: outR,
                        innerR: innerR
                    })
					.attr({ m: i+'-'+j,data: ((e-s)/360*100).toFixed(0)+'%',pos: showDot[0]+'-'+showDot[1] })
					.css({ zIndex: 5,cursor: 'pointer' })
					.on('mouseover',function () {
					    this.setOpacity(0.85);
					    var pos=this.attr('pos').split('-');
					    self.showTooltip(o,pos[0],pos[1],this.attr('data'))
					})
					.on('mouseout',function (e) {
					    var elem=e.toElement||e.relatedTarget;
					    if(!elem||elem.getAttribute('m')===this[0].getAttribute('m'))
					        return;
					    this.setOpacity(1);
					})
					.appendTo(piesGroup);
                    self.baseDraw.span(self,{
                        x: x,
                        y: y,
                        fill: '#fff',
                        'text-anchor': 'left'
                    })
						.attr({ m: i+'-'+j })
						.css({
						    fontFamily: 'Verdana,Arial,Helvetica,sans-serif',
						    fontSize: '12px',
						    position: 'absolute',
						    color: '#fff',
						    cursor: 'pointer',
						    zIndex: 10
						})
						.addText(name)
						.appendTo(piesGroup);
                });

            });

        },
        drawSegment: function () {

            var self=this,
				opts=this.opts,
				draw=opts.draw,
				chartData=opts.chartData,
				typeIsX=opts.charts.panel==='x',
				columnPad=5,
				prev=[],
				columnSize= ~ ~(typeIsX?draw.xPices:draw.yPices)-columnPad*2;

            each(chartData,function (i,c) {
                if(i in self.hideList)
                    return;

                var id='chart'+i,
					segmentGroup=self.mainGroup[id]?self.mainGroup[id].chart:self.group(id).appendTo();

                self.mainGroup[id]={ chart: segmentGroup,show: true };

                each(c.data,function (j,d) {
                    if(d===null||d===0)
                        return;

                    if(typeIsX) {
                        var start=draw.startX+columnPad,
							x= ~ ~(start+j*draw.xPices).toFixed(0),
							y= ~ ~(draw.startY-(prev[j]?prev[j]:0)).toFixed(0),
							size= ~ ~(d*draw.yPices/draw.feed).toFixed(0),
							path=[
								M,
								x,
								y,
								L,
								x,
								y-size,
								L,
								x+columnSize,
								y-size,
								L,
								x+columnSize,
								y,
								seal
							];
                        var pos=[x+columnSize/2,y-size];
                    } else {
                        var start=draw.startY-columnPad,
							x= ~ ~(draw.startX+(prev[j]?prev[j]:0)).toFixed(0),
							y= ~ ~(start-j*draw.yPices).toFixed(0),
							size= ~ ~(d*draw.xPices/draw.feed).toFixed(0),
							path=[
								M,
								x,
								y,
								L,
								x+size,
								y,
								L,
								x+size,
								y-columnSize,
								L,
								x,
								y-columnSize,
								seal
							];
                        var pos=[x+size/2,y-columnSize];
                    }

                    self.baseDraw.path(self,{
                        border: 0,
                        isfill: true,
                        fillColor: c.color,
                        opacity: 1,
                        path: path
                    })
					.attr({ data: d,pos: pos[0]+'-'+pos[1] })
					.on('mouseover',function () {
					    this.setOpacity(0.85);
					    var pos=this.attr('pos').split('-');
					    self.showTooltip(chartData[i],pos[0],pos[1],this.attr('data'))
					})
					.on('mouseout',function () {
					    this.setOpacity(1);
					})
					.css({ zIndex: 5,cursor: 'pointer',left: '0px',top: '0px' })
					.appendTo(segmentGroup);

                    prev[j]
						?prev[j]=prev[j]+size
						:prev[j]=size;

                });
            });
        },
        baseDraw: {
            rect: function (o,config) {
                return o.createElement('rect')
					.attr({
					    rx: config.arc*30||5,
					    ry: config.arc*30||5,
					    width: config.width||50,
					    height: config.height||50,
					    fill: config.fill||'#fff',
					    'fill-opacity': config.opacity||0.85,
					    'stroke-width': config.border||2,
					    stroke: config.borderColor||'#606060',
					    transform: 'translate('+(config.left||0)+','+(config.top||0)+')'
					});
            },
            text: function (o,config) {
                return o.createElement('text')
					.attr(config);
            },
            span: function (o,config,v) {
                return o.createElement('text')
					.attr(config)
					.attr({
					    y: config.y+(v||15)
					});
            },
            path: function (o,config) {
                var set={};
                set['stroke-width']=config.border;
                set.stroke=config.borderColor||'#C0C0C0';
                set.fill=config.isfill?config.fillColor:'none';
                set.d=config.path;
                config.opacity
					&&(set['fill-opacity']=config.opacity);

                return o.createElement('path')
					.attr(set);
            },
            circle: function (o,config) {
                var set={};
                set.cx=config.x;
                set.cy=config.y;
                set['stroke-width']=0;
                set.stroke=config.borderColor||'#C0C0C0';
                set.r=config.r;
                set.fill=config.fillColor;
                return o.createElement('circle')
					.attr(set);
            },
            square: function (o,config) {
                var x=config.x,
					y=config.y,
					r=config.r,
					color=config.fillColor,
					len=r,
					path=[
						M,
						(x-len).toFixed(0),
						(y-len).toFixed(0),
						L,
						(x+len).toFixed(0),
						(y-len).toFixed(0),
						(x+len).toFixed(0),
						(y+len).toFixed(0),
						(x-len).toFixed(0),
						(y+len).toFixed(0),
						seal
					];
                return o.baseDraw.path(o,{
                    border: 1,
                    borderColor: color,
                    isfill: true,
                    fillColor: color,
                    path: path
                });
            },
            triangle: function (o,config) {
                var x=config.x,
					y=config.y,
					r=config.r+0.1,
					color=config.fillColor,
					path=[
						M,
						x.toFixed(0),
						(y-1.33*r).toFixed(0),
						L,
						(x+r).toFixed(0),
						(y+0.67*r).toFixed(0),
						(x-r).toFixed(0),
						(y+0.67*r).toFixed(0),
						seal
					];
                return o.baseDraw.path(o,{
                    border: 1,
                    borderColor: color,
                    isfill: true,
                    fillColor: color,
                    path: path
                });
            },
            diamond: function (o,config) {
                var x=config.x,
					y=config.y,
					r=1.35*config.r,
					color=config.fillColor,
					path=[
						M,
						x.toFixed(0),
						(y-r).toFixed(0),
						L,
						(x+r).toFixed(0),
						y.toFixed(0),
						x.toFixed(0),
						(y+r).toFixed(0),
						(x-r).toFixed(0),
						y.toFixed(0),
						seal
					];
                return o.baseDraw.path(o,{
                    border: 1,
                    borderColor: color,
                    isfill: true,
                    fillColor: color,
                    path: path
                });
            },
            pie: function (o,config) {
                //config,s,e,r,index
                var opts=o.opts,
					s=config.s,
					r=config.r,
					e=config.e-0.000001,
					id='chart'+config.index,
					area=opts.area,
					rx=area.centerX,
					ry=area.centerY,
					cosStart=mathCos(s),
					sinStart=mathSin(s),
					cosEnd=mathCos(e),
					sinEnd=mathSin(e),
					color=config.config.color,
					innerR=config.innerR,
					longArc=e-s<Math.PI?0:1,
					path=[
						M,
						rx+r*cosStart,
						ry+r*sinStart,
						'A',
						r,
						r,
						0,
						longArc,
						1,
						rx+r*cosEnd,
						ry+r*sinEnd,
						L,
						rx+innerR*cosEnd,
						ry+innerR*sinEnd,
						'A', // arcTo
						innerR, // x radius
						innerR, // y radius
						0, // slanting
						longArc, // long or short arc
						0, // clockwise
						rx+innerR*cosStart,
						ry+innerR*sinStart,
						'Z'
					];

                return o.baseDraw.path(o,{
                    border: 1,
                    border: '#fff',
                    isfill: true,
                    fillColor: color,
                    opacity: 1,
                    path: path

                })

            }
        }
    };

    //---------------------------------------------------------------------------------------------------
    //如果是vml 修改smipleChart.prototype中的一些方法
    !hasSVG
		&&extend(smipleChart.prototype.baseDraw,{
		    rect: function (o,config) {
		        var attr={},
					css={};
		        attr.arcsize=config.arc||0.2+'';
		        if(config.fill==='none') {
		            attr.filled='f'
		        } else {
		            attr.filled='t';
		            attr.fillcolor=config.fill||'#fff';
		        }

		        attr.strokeWeight=config.border||2;
		        attr.strokeColor=config.borderColor||'#606060';
		        css.width=config.width||50+'px';
		        css.height=config.height||50+'px';
		        css.zIndex=10;
		        css.left=config.left||0+'px';
		        css.top=config.top||0+'px';

		        return o.createElement('roundrect')
					.attr(attr)
					.css(css);
		    },
		    text: function (o,config) {
		        return o.createElement('TextBox')
					.attr({ inset: "2px,2px,2px,2px" })
					.css({ zIndex: 200 })
		    },
		    span: function (o,config) {
		        return o.createElement('span').
					css({
					    position: 'absolute',
					    left: config.x+'px',
					    top: config.y+'px'
					})
		    },
		    path: function (o,config) {
		        var attr={},
					width=o.width,
					height=o.height,
					css={
					    width: width+'px',
					    height: height+'px'
					};

		        if(config.border===0) {
		            attr.Stroked='f';
		            attr.strokeWeight=0;
		        } else {
		            attr.strokeWeight=config.border||1;
		        }
		        attr.strokeColor=config.borderColor||"#C0C0C0";
		        attr.filled=config.isfill?'t':'f';
		        attr.filled==='t'
					&&(attr.fillcolor=config.fillColor||"#C0C0C0");
		        attr.coordsize=width+','+height;
		        attr.path=config.path;
		        var elem=o.createElement()
					.attr(attr)
					.css(css);
		        if(config.opacity) {
		            var fill=o.createElement('fill')
						.attr({
						    type: 'fill',
						    color: config.fillColor||"#C0C0C0",
						    opacity: config.opacity
						})
						.appendTo(elem);
		            //那这个对象的一个属性引用设置透明的元素 以后会用到
		            elem.opacity=fill[0];
		        }
		        return elem;

		    },
		    circle: function (o,config) {
		        var width=o.width,
					height=o.height,
					attr={
					    strokeWeight: 1,
					    coordsize: width+','+height,
					    filled: 't'
					},
					css={
					    width: width+'px',
					    height: height+'px'
					}
		        x=config.x,
					y=config.y,
					r=config.r;
		        attr.strokeColor=attr.fillcolor=config.fillColor

		        attr.path=[
					'wa', // clockwisearcto
					x-r, // left
					y-r, // top
					x+r, // right
					y+r, // bottom
					x+r, // start x
					y,     // start y
					x+r, // end x
					y,     // end y
					'e' // close								
				];
		        return o.createElement()
					.attr(attr)
					.css(css)
		    },
		    pie: function (o,config) {
		        ////config,s,e,r,index

		        var opts=o.opts,
					area=opts.area,
					r=config.r,
					rx=area.centerX,
					ry=area.centerY,
					innerR=config.innerR||0,
					sDot=angle(r,[rx,ry],s,2),
					eDot=angle(r,[rx,ry],e,2),
					color=config.config.color,
					s=config.s,
					e=config.e,
					e=e-s==2*Math.PI?e-0.001:e,
					cosStart=mathCos(s),
					sinStart=mathSin(s),
					cosEnd=mathCos(e),
					sinEnd=mathSin(e),


					path=[
						'wa', // clockwisearcto
						(rx-r).toFixed(0), // left
						(ry-r).toFixed(0), // top
						(rx+r).toFixed(0), // right
						(ry+r).toFixed(0), // bottom
						(rx+r*cosStart).toFixed(0), // start x
						(ry+r*sinStart).toFixed(0), // start y
						(rx+r*cosEnd).toFixed(0), // end x
						(ry+r*sinEnd).toFixed(0), // end y								

						'at', // clockwisearcto
						(rx-innerR).toFixed(0), // left
						(ry-innerR).toFixed(0), // top
						(rx+innerR).toFixed(0), // right
						(ry+innerR).toFixed(0), // bottom
						(rx+innerR*cosEnd).toFixed(0), // start x
						(ry+innerR*sinEnd).toFixed(0), // start y
						(rx+innerR*cosStart).toFixed(0), // end x
						(ry+innerR*sinStart).toFixed(0), // end y

						'x', // finish path
						'e' // close									 
					];

		        return o.baseDraw.path(o,{
		            border: 1,
		            border: '#fff',
		            isfill: true,
		            fillColor: color,
		            opacity: 1,
		            path: path
		        })

		    }
		});

})(document);
