section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V14internal_printPhx

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
xor rcx, rcx
cmp rcx, rbx
jge _V8pidigitsx_rPh_L1
_V8pidigitsx_rPh_L0:
mov rdx, rcx
mov r12, rcx
mov rcx, rdi
mov r8, 20
mov r13, rax
call _VN5ArrayIxE3setExx
add r12, 1
mov rax, r13
mov rcx, r12
cmp rcx, rbx
jl _V8pidigitsx_rPh_L0
_V8pidigitsx_rPh_L1:
xor rdx, rdx
cmp rdx, rsi
jge _V8pidigitsx_rPh_L5
_V8pidigitsx_rPh_L4:
xor rcx, rcx
xor r8, r8
cmp rcx, rbx
jge _V8pidigitsx_rPh_L8
_V8pidigitsx_rPh_L7:
mov r12, rbx
sub r12, rcx
sub r12, 1
mov r13, r12
sal r13, 1
add r13, 1
mov r14, rdx
mov rdx, rcx
mov r15, rcx
mov rcx, rdi
mov qword [rsp+80], rax
mov qword [rsp+72], r8
call _VN5ArrayIxE3getEx_rx
mov rcx, [rsp+72]
add rax, rcx
mov rdx, rcx
mov rcx, rdi
mov r8, rdx
mov rdx, r15
mov r9, r8
mov r8, rax
mov qword [rsp+72], r9
call _VN5ArrayIxE3setExx
mov rcx, rdi
mov rdx, r15
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
cqo
idiv r13
mov rcx, rdi
mov rdx, r15
mov qword [rsp+64], rax
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
cqo
idiv r13
mov rcx, rbp
mov r8, rdx
mov rdx, r15
call _VN5ArrayIxE3setExx
mov rcx, [rsp+64]
imul rcx, r12
add r15, 1
mov r8, rcx
mov rax, [rsp+80]
mov rcx, r15
mov rdx, r14
cmp rcx, rbx
jl _V8pidigitsx_rPh_L7
_V8pidigitsx_rPh_L8:
mov r9, rbx
sub r9, 1
mov r12, rcx
mov rcx, rdi
mov r13, rdx
mov rdx, r9
mov r14, rax
mov r15, r8
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
mov rcx, rbx
sub rcx, 1
mov rdx, rbx
sub rdx, 1
mov r8, rcx
mov rcx, rdi
mov qword [rsp+56], r8
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
cqo
mov r8, 10
idiv r8
mov rcx, rbp
mov r8, rdx
mov rdx, [rsp+56]
call _VN5ArrayIxE3setExx
xor rax, rax
cmp rax, rbx
jge _V8pidigitsx_rPh_L12
_V8pidigitsx_rPh_L11:
mov rcx, rbp
mov rdx, rax
mov r12, rax
call _VN5ArrayIxE3getEx_rx
imul rax, 10
mov rcx, rdi
mov rdx, r12
mov r8, rax
call _VN5ArrayIxE3setExx
add r12, 1
mov rax, r12
cmp rax, rbx
jl _V8pidigitsx_rPh_L11
_V8pidigitsx_rPh_L12:
add r13, 1
mov rcx, rax
mov rdx, r13
mov rax, r14
cmp rdx, rsi
jl _V8pidigitsx_rPh_L4
_V8pidigitsx_rPh_L5:
mov r8, rsi
sal r8, 3
mov r12, rcx
mov rcx, r8
mov r13, rax
mov r14, rdx
call _VN5ArrayIhE4initEx_rPS_
mov rcx, rsi
sub rcx, 1
xor rdx, rdx
test rcx, rcx
jl _V8pidigitsx_rPh_L17
_V8pidigitsx_rPh_L16:
mov rbx, rdx
mov rdx, rcx
mov rsi, rcx
mov rcx, r13
mov rdi, rax
call _VN5ArrayIxE3getEx_rx
add rax, rbx
mov rcx, r13
mov rdx, rsi
mov r8, rax
call _VN5ArrayIxE3setExx
mov rcx, r13
mov rdx, rsi
call _VN5ArrayIxE3getEx_rx
mov rcx, 1844674407370955162
mul rcx
mov rbx, rdx
sar rbx, 63
add rbx, rdx
mov rcx, r13
mov rdx, rsi
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
cqo
mov rbp, 10
idiv rbp
mov rcx, 48
add rcx, rdx
mov r8, rcx
mov rdx, rsi
mov rcx, rdi
call _VN5ArrayIhE3setExh
sub rsi, 1
mov rax, rdi
mov rcx, rsi
mov rdx, rbx
test rcx, rcx
jge _V8pidigitsx_rPh_L16
_V8pidigitsx_rPh_L17:
mov rax, [rax+8]
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
movzx rdx, byte [rcx+rax]
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
sub rsp, 48
mov rbx, rcx
call _V9length_ofPh_rx
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 48
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
mov qword [rsi+8], rax
mov qword [rsi+16], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN5ArrayIxE3setExx:
sal rdx, 3
mov rcx, [rcx+8]
mov qword [rcx+rdx], r8
ret

_VN5ArrayIxE3getEx_rx:
sal rdx, 3
mov r8, [rcx+8]
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
mov qword [rsi+8], rax
mov qword [rsi+16], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN5ArrayIhE3setExh:
mov rcx, [rcx+8]
mov byte [rcx+rdx], r8b
ret

section .data

_VN5Array_configuration:
dq _VN5Array_descriptor

_VN5Array_descriptor:
dq _VN5Array_descriptor_0
dd 8
dd 0

_VN5Array_descriptor_0:
db 'Array', 0

_VN6String_configuration:
dq _VN6String_descriptor

_VN6String_descriptor:
dq _VN6String_descriptor_0
dd 16
dd 0

_VN6String_descriptor_0:
db 'String', 0

_VN5ArrayIxE_configuration:
dq _VN5ArrayIxE_descriptor

_VN5ArrayIxE_descriptor:
dq _VN5ArrayIxE_descriptor_0
dd 24
dd 0

_VN5ArrayIxE_descriptor_0:
db 'Array<large>', 0

_VN5ArrayIhE_configuration:
dq _VN5ArrayIhE_descriptor

_VN5ArrayIhE_descriptor:
dq _VN5ArrayIhE_descriptor_0
dd 24
dd 0

_VN5ArrayIhE_descriptor_0:
db 'Array<u8>', 0