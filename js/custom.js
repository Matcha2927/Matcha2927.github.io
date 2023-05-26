 document.addEventListener('visibilitychange', function() {
 	var isHidden = document.hidden;
 	if (isHidden) {
 		document.title = '你不爱我了(ó﹏ò｡)';
 	} else {
 		document.title = '抹茶蛋糕';
 	}
 });