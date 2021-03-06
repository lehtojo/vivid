﻿Holder {
	value: large
	other: large
}

# Without reordering:
#
# mov r9, rdx ; copy
# sal r9, 2 ; (copied a) * 4
# add r9, r8 ; (copied a) * 4 + b
# mov qword [rcx], rdx ; save the original a
# mov qword [rcx+8], r9 ; save the d
# mov rax, rdx
# ret
#
# With ordering:
#
# mov qword [rcx], rdx ; save the original a first
# mov rax, rdx
# sal rdx, 2
# add rdx, r8
# mov qword [rcx], rdx
#

f(holder, a, b) {
	d = a * 4 + b
	holder.value = a
	holder.other = d
	
	=> a
}

init() {
	holder = Holder()
	=> f(holder, 7, 10) - 7
}