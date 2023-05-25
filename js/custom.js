 document.addEventListener('visibilitychange', function() {
 	var isHidden = document.hidden;
 	if (isHidden) {
 		document.title = '你不爱我了qwq';
 	} else {
 		document.title = '抹茶蛋糕';
 	}
 });