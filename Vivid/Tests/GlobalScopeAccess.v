namespace foo.bar {
	Test {
		target(): _ {
			console.write_line(':^(')
		}

		init() {
			global.target()
		}
	}
}

target(): _ {
	console.write_line(':^)')
}

init(): i64 {
	foo.bar.Test()
	return 0
}