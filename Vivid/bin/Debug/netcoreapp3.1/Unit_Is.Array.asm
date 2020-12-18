.section .text
.intel_syntax noprefix
.global _VN5ArrayIP6UsableE3setExS0_
_VN5ArrayIP6UsableE3setExS0_:
push rbx
push rsi
push rdi
sub rsp, 32
xor rax, rax
test rdx, rdx
jl _VN5ArrayIP6UsableE3setExS0__L0
cmp rdx, [rcx+16]
jge _VN5ArrayIP6UsableE3setExS0__L0
mov rax, 1
_VN5ArrayIP6UsableE3setExS0__L0:
mov rbx, rcx
mov rcx, rax
mov rsi, rdx
lea rdx, [rip+_VN5ArrayIP6UsableE3setExS0__S0]
mov rdi, r8
call _V7requirebPh
mov rcx, [rbx+8]
mov qword ptr [rcx+rsi*8], rdi
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _VN5ArrayIP6UsableE3getEx_rS0_
_VN5ArrayIP6UsableE3getEx_rS0_:
push rbx
push rsi
sub rsp, 40
xor rax, rax
test rdx, rdx
jl _VN5ArrayIP6UsableE3getEx_rS0__L0
cmp rdx, [rcx+16]
jge _VN5ArrayIP6UsableE3getEx_rS0__L0
mov rax, 1
_VN5ArrayIP6UsableE3getEx_rS0__L0:
mov rbx, rcx
mov rcx, rax
mov rsi, rdx
lea rdx, [rip+_VN5ArrayIP6UsableE3getEx_rS0__S0]
call _V7requirebPh
mov rcx, [rbx+8]
mov rax, [rcx+rsi*8]
add rsp, 40
pop rsi
pop rbx
ret

.global _VN5ArrayIP6UsableE4initEx_rS1_
_VN5ArrayIP6UsableE4initEx_rS1_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
call _V8allocatex_rPh
xor rcx, rcx
test rbx, rbx
jl _VN5ArrayIP6UsableE4initEx_rS1__L0
mov rcx, 1
_VN5ArrayIP6UsableE4initEx_rS1__L0:
lea rdx, [rip+_VN5ArrayIP6UsableE4initEx_rS1__S0]
mov rsi, rax
call _V7requirebPh
mov rcx, rbx
sal rcx, 3
call _V8allocatex_rPh
mov qword ptr [rsi+8], rax
mov qword ptr [rsi+16], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

.global _V7requirebPh
_V7requirebPh:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
test rbx, rbx
jne _V7requirebPh_L0
mov rcx, rsi
call _V7printlnPh
mov rcx, 1
call _V4exitx
_V7requirebPh_L0:
add rsp, 40
pop rsi
pop rbx
ret

.section .data

_VN5Array_configuration:
.quad _VN5Array_descriptor

_VN5Array_descriptor:
.quad _VN5Array_descriptor_0
.long 8
.long 0

_VN5Array_descriptor_0:
.ascii "Array"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5Sheet_configuration:
.quad _VN5Sheet_descriptor

_VN5Sheet_descriptor:
.quad _VN5Sheet_descriptor_0
.long 8
.long 0

_VN5Sheet_descriptor_0:
.ascii "Sheet"
.byte 0
.byte 1
.byte 2
.byte 0

_VN3Box_configuration:
.quad _VN3Box_descriptor

_VN3Box_descriptor:
.quad _VN3Box_descriptor_0
.long 8
.long 0

_VN3Box_descriptor_0:
.ascii "Box"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5ArrayIP6UsableE_configuration:
.quad _VN5ArrayIP6UsableE_descriptor

_VN5ArrayIP6UsableE_descriptor:
.quad _VN5ArrayIP6UsableE_descriptor_0
.long 24
.long 0

_VN5ArrayIP6UsableE_descriptor_0:
.ascii "Array<Usable>"
.byte 0
.byte 1
.byte 2
.byte 0

.balign 16
_VN5ArrayIP6UsableE3setExS0__S0:
.ascii "Index out of bounds"
.byte 0
.balign 16
_VN5ArrayIP6UsableE3getEx_rS0__S0:
.ascii "Index out of bounds"
.byte 0
.balign 16
_VN5ArrayIP6UsableE4initEx_rS1__S0:
.ascii "Tried to create a standard array but its size was a negative value"
.byte 0

