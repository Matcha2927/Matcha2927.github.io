 document.addEventListener('visibilitychange', function() {
 	var isHidden = document.hidden;
 	if (isHidden) {
 		document.title = '你不爱我了(ó﹏ò｡)';
 	} else {
 		document.title = '抹茶蛋糕';
 	}
 });

 var randomNum = function(minNum, maxNum) {
 	switch (arguments.length) {
 		case 1:
 			return parseInt(Math.random() * minNum + 1, 10);
 			break;
 		case 2:
 			return parseInt(Math.random() * (maxNum - minNum + 1) + minNum, 10);
 			break;
 		default:
 			return 0;
 			break;
 	}
 }

 //躲猫猫的「猫猫」形象出自游戏「Don't catch Cats」(https://apps.apple.com/app/dont-catch-cats/id1375311035)。
 var duoMaomao = function() {
 	var maomao = $('#maomao');
 	maomao.css('bottom', randomNum(5, 80) + 'vh');
 }

 // 创建<div>标签
 var divElement = document.createElement("div");

 // 设置id属性
 divElement.setAttribute("id", "maomao");

 // 设置onMouseOut属性
 divElement.setAttribute("onMouseOut", "duoMaomao()");

 // 将<div>标签添加到<body>元素中
 document.body.appendChild(divElement);