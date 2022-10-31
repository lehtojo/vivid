export test_1(a: i32, b: i32) { return -(-(-(-(a + b)))) }
export test_2(a: i32, b: i32) { return !(!(!(!(a + b)))) }
export test_3(a: i32, b: i32) { return -(-(-(a * b))) }
export test_4(a: i32, b: i32) { return !(!(!(a * b))) }

init() {
	console.write_line(test_1(3, 7))
	console.write_line(test_2(10, 42))
	console.write_line(test_3(-42, 7))
	console.write_line(test_4(10, -3))
	return 0
}