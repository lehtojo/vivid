init() {
	console.write_line('String closures: \', \"')
	console.write_line("Single-line comment opening: #")
	console.write("Parenthesis closures: ), ], ")
	console.put(`}`)
	console.put(`\n`)
	console.write_line(
		"Multi-line comment closures: ###"
		###
		This is a multi-line comment.

		Compiler should skip these commented characters: (, }, ", '
		###

		### This a single-line multi-line comment :^) ###
	)

	return 0
}