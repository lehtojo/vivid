.section .text
.intel_syntax noprefix
.section .data
_VN6Random_a: .quad 0
_VN6Random_b: .quad 0
_VN6Random_c: .quad 0
_VN6Random_n: .quad 0

_VN6Random_configuration:
.quad _VN6Random_descriptor

_VN6Random_descriptor:
.quad _VN6Random_descriptor_0
.long 8
.long 0

_VN6Random_descriptor_0:
.ascii "Random"
.byte 0
.byte 1
.byte 2
.byte 0

