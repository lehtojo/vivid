.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V14internal_printPhx
.extern _V17internal_allocatex_rPh

_V8pidigitsx_rPh:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 88
add rcx, 1
mov rbx, rcx
imul rbx, 10
mov rax, rbx
mov r8, 6148914691236517206
mul r8
mov rbx, rdx
sar rbx, 63
add rbx, rdx
add rbx, 2
mov rsi, rcx
mov rcx, rbx
call _VN5ArrayIxE4initEx_rPS_
mov rcx, rbx
mov rdi, rax
call _VN5ArrayIxE4initEx_rPS_
mov rcx, rsi
mov rbp, rax
call _VN5ArrayIxE4initEx_rPS_
xor r12, r12
cmp r12, rbx
jge _V8pidigitsx_rPh_L1
_V8pidigitsx_rPh_L0:
mov rcx, rdi
mov rdx, r12
mov r8, 20
mov r13, rax
call _VN5ArrayIxE3setExx
add r12, 1
mov rax, r13
cmp r12, rbx
jl _V8pidigitsx_rPh_L0
_V8pidigitsx_rPh_L1:
xor r13, r13
mov r14, rax
cmp r13, rsi
jge _V8pidigitsx_rPh_L5
_V8pidigitsx_rPh_L4:
xor r12, r12
xor r15, r15
cmp r12, rbx
jge _V8pidigitsx_rPh_L8
_V8pidigitsx_rPh_L7:
mov r8, rbx
sub r8, r12
sub r8, 1
mov r9, r8
sal r9, 1
add r9, 1
mov rcx, rdi
mov rdx, r12
mov qword ptr [rsp+80], r8
mov qword ptr [rsp+72], r9
call _VN5ArrayIxE3getEx_rx
add rax, r15
mov rcx, rdi
mov rdx, r12
mov r8, rax
call _VN5ArrayIxE3setExx
mov rcx, rdi
mov rdx, r12
call _VN5ArrayIxE3getEx_rx
cqo
mov rcx, [rsp+72]
idiv rcx
mov qword ptr [rsp+72], rcx
mov rcx, rdi
mov rdx, r12
mov qword ptr [rsp+64], rax
call _VN5ArrayIxE3getEx_rx
cqo
idiv qword ptr [rsp+72]
mov rcx, rbp
mov r8, rdx
mov rdx, r12
call _VN5ArrayIxE3setExx
mov r15, [rsp+64]
imul r15, [rsp+80]
add r12, 1
cmp r12, rbx
jl _V8pidigitsx_rPh_L7
_V8pidigitsx_rPh_L8:
mov rdx, rbx
sub rdx, 1
mov rcx, rdi
call _VN5ArrayIxE3getEx_rx
mov rcx, 1844674407370955162
mul rcx
mov rcx, rdx
sar rcx, 63
add rcx, rdx
mov r8, rcx
mov rdx, r13
mov rcx, r14
call _VN5ArrayIxE3setExx
mov r8, rbx
sub r8, 1
mov rdx, rbx
sub rdx, 1
mov rcx, rdi
mov qword ptr [rsp+48], r8
call _VN5ArrayIxE3getEx_rx
cqo
mov rcx, 10
idiv rcx
mov rcx, rbp
mov r8, rdx
mov rdx, [rsp+48]
call _VN5ArrayIxE3setExx
xor r12, r12
cmp r12, rbx
jge _V8pidigitsx_rPh_L12
_V8pidigitsx_rPh_L11:
mov rcx, rbp
mov rdx, r12
call _VN5ArrayIxE3getEx_rx
imul rax, 10
mov rcx, rdi
mov rdx, r12
mov r8, rax
call _VN5ArrayIxE3setExx
add r12, 1
cmp r12, rbx
jl _V8pidigitsx_rPh_L11
_V8pidigitsx_rPh_L12:
add r13, 1
cmp r13, rsi
jl _V8pidigitsx_rPh_L4
_V8pidigitsx_rPh_L5:
mov rcx, rsi
sal rcx, 3
call _VN5ArrayIhE4initEx_rPS_
mov r13, rsi
sub r13, 1
xor r15, r15
mov qword ptr [rsp+56], rbx
mov rbx, rax
test r13, r13
jl _V8pidigitsx_rPh_L17
_V8pidigitsx_rPh_L16:
mov rcx, r14
mov rdx, r13
call _VN5ArrayIxE3getEx_rx
add rax, r15
mov rcx, r14
mov rdx, r13
mov r8, rax
call _VN5ArrayIxE3setExx
mov rcx, r14
mov rdx, r13
call _VN5ArrayIxE3getEx_rx
mov rcx, 1844674407370955162
mul rcx
mov r15, rdx
sar r15, 63
add r15, rdx
mov rcx, r14
mov rdx, r13
call _VN5ArrayIxE3getEx_rx
cqo
mov rcx, 10
idiv rcx
mov rcx, 48
add rcx, rdx
mov r8, rcx
mov rdx, r13
mov rcx, rbx
call _VN5ArrayIhE3setExh
sub r13, 1
test r13, r13
jge _V8pidigitsx_rPh_L16
_V8pidigitsx_rPh_L17:
mov rax, [rbx+8]
add rsp, 88
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V4initv_rx:
sub rsp, 40
mov rcx, 3141
call _V8pidigitsx_rPh
mov rcx, rax
call _V5printPh
xor rax, rax
add rsp, 40
ret

_V9length_ofPh_rx:
xor rax, rax
_V9length_ofPh_rx_L1:
_V9length_ofPh_rx_L0:
movzx rdx, byte ptr [rcx+rax]
test rdx, rdx
jne _V9length_ofPh_rx_L3
ret
_V9length_ofPh_rx_L3:
add rax, 1
jmp _V9length_ofPh_rx_L0
_V9length_ofPh_rx_L2:
ret

_V5printPh:
push rbx
sub rsp, 32
mov rbx, rcx
call _V9length_ofPh_rx
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 32
pop rbx
ret

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rip+_VN10Allocation_current]
test r8, r8
je _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword ptr [r8+16], r9
lea r9, [rdx+rcx]
mov rax, [r8+8]
add rax, rdx
add rsp, 40
pop rsi
pop rbx
ret
_V8allocatex_rPh_L0:
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov rcx, 24
mov rsi, rax
call _V17internal_allocatex_rPh
mov qword ptr [rax+8], rsi
mov qword ptr [rax+16], rbx
mov qword ptr [rip+_VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhS__rx:
push rbx
push rsi
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte ptr [r9]
xor rax, rax
_V8inheritsPhS__rx_L1:
_V8inheritsPhS__rx_L0:
movzx rcx, byte ptr [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhS__rx_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhS__rx_L7:
_V8inheritsPhS__rx_L6:
movzx r11, byte ptr [r8+rax]
movzx rsi, byte ptr [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhS__rx_L9
cmp r11, 1
jne _V8inheritsPhS__rx_L9
test rsi, rsi
jne _V8inheritsPhS__rx_L9
mov rax, 1
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L9:
jmp _V8inheritsPhS__rx_L6
_V8inheritsPhS__rx_L8:
jmp _V8inheritsPhS__rx_L3
_V8inheritsPhS__rx_L4:
cmp rcx, 2
jne _V8inheritsPhS__rx_L3
xor rax, rax
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L3:
jmp _V8inheritsPhS__rx_L0
_V8inheritsPhS__rx_L2:
pop rsi
pop rbx
ret

_VN5ArrayIxE4initEx_rPS_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
call _V8allocatex_rPh
mov rcx, rbx
sal rcx, 3
mov rsi, rax
call _V8allocatex_rPh
mov qword ptr [rsi+8], rax
mov qword ptr [rsi+16], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN5ArrayIxE3setExx:
mov r9, [rcx+8]
sal rdx, 3
mov qword ptr [r9+rdx], r8
ret

_VN5ArrayIxE3getEx_rx:
mov r8, [rcx+8]
sal rdx, 3
mov rax, [r8+rdx]
ret

_VN5ArrayIhE4initEx_rPS_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
call _V8allocatex_rPh
mov rcx, rbx
mov rsi, rax
call _V8allocatex_rPh
mov qword ptr [rsi+8], rax
mov qword ptr [rsi+16], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN5ArrayIhE3setExh:
mov r9, [rcx+8]
mov byte ptr [r9+rdx], r8b
ret

.section .data

_VN10Allocation_current:
.quad 0

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

_VN6String_configuration:
.quad _VN6String_descriptor

_VN6String_descriptor:
.quad _VN6String_descriptor_0
.long 16
.long 0

_VN6String_descriptor_0:
.ascii "String"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4Page_configuration:
.quad _VN4Page_descriptor

_VN4Page_descriptor:
.quad _VN4Page_descriptor_0
.long 24
.long 0

_VN4Page_descriptor_0:
.ascii "Page"
.byte 0
.byte 1
.byte 2
.byte 0

_VN10Allocation_configuration:
.quad _VN10Allocation_descriptor

_VN10Allocation_descriptor:
.quad _VN10Allocation_descriptor_0
.long 8
.long 0

_VN10Allocation_descriptor_0:
.ascii "Allocation"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5ArrayIxE_configuration:
.quad _VN5ArrayIxE_descriptor

_VN5ArrayIxE_descriptor:
.quad _VN5ArrayIxE_descriptor_0
.long 24
.long 0

_VN5ArrayIxE_descriptor_0:
.ascii "Array<large>"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5ArrayIhE_configuration:
.quad _VN5ArrayIhE_descriptor

_VN5ArrayIhE_descriptor:
.quad _VN5ArrayIhE_descriptor_0
.long 24
.long 0

_VN5ArrayIhE_descriptor_0:
.ascii "Array<u8>"
.byte 0
.byte 1
.byte 2
.byte 0