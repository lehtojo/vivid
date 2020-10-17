square(num) {
	a = num
	num = 5 + a
	=> a + num
}

write_decoy(num) {
	a = num
	a = 5
	=> num + num + a * 2
}

init() {
	square(7)
	write_decoy(7)
}