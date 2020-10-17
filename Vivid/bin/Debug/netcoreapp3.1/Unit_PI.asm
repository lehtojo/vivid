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
call _VN5ArrayIxE4initEx_rPh
mov rcx, rbx
mov rdi, rax
call _VN5ArrayIxE4initEx_rPh
mov rcx, rsi
mov rbp, rax
call _VN5ArrayIxE4initEx_rPh
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
mov rcx, rbx
sub rcx, r12
sub rcx, 1
mov rdx, rcx
sal rdx, 1
add rdx, 1
mov r8, rcx
mov rcx, rdi
mov r9, rdx
mov rdx, r12
mov qword [rsp+80], r8
mov qword [rsp+72], r9
call _VN5ArrayIxE3getEx_rx
add rax, r15
mov rcx, rdi
mov rdx, r12
mov r8, rax
call _VN5ArrayIxE3setExx
mov rcx, rdi
mov rdx, r12
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
xor rdx, rdx
mov r8, [rsp+72]
idiv r8
mov rcx, rdi
mov rdx, r12
mov qword [rsp+64], rax
mov qword [rsp+72], r8
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
xor rdx, rdx
idiv qword [rsp+72]
mov rcx, rbp
mov r8, rdx
mov rdx, r12
call _VN5ArrayIxE3setExx
mov rcx, [rsp+64]
imul rcx, [rsp+80]
add r12, 1
mov r15, rcx
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
mov rcx, rbx
sub rcx, 1
mov rdx, rbx
sub rdx, 1
mov r8, rcx
mov rcx, rdi
mov qword [rsp+48], r8
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
xor rdx, rdx
mov r8, 10
idiv r8
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
call _VN5ArrayIhE4initEx_rPh
mov rcx, rsi
sub rcx, 1
mov r13, rcx
xor r15, r15
mov qword [rsp+56], rbx
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
mov rsi, rdx
sar rsi, 63
add rsi, rdx
mov rcx, r14
mov rdx, r13
call _VN5ArrayIxE3getEx_rx
mov rcx, rax
xor rdx, rdx
mov rdi, 10
idiv rdi
mov rcx, 48
add rcx, rdx
mov r8, rcx
mov rdx, r13
mov rcx, rbx
call _VN5ArrayIhE3setExh
sub r13, 1
mov r15, rsi
test r13, r13
jge _V8pidigitsx_rPh_L16
_V8pidigitsx_rPh_L17:
mov rax, [rbx]
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

_VN5ArrayIxE4initEx_rPh:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov rcx, rbx
sal rcx, 3
mov rsi, rax
call _V8allocatex_rPh
mov qword [rsi], rax
mov qword [rsi+8], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN5ArrayIxE3setExx:
sal rdx, 3
mov rax, [rcx]
mov qword [rax+rdx], r8
ret

_VN5ArrayIxE3getEx_rx:
sal rdx, 3
mov r8, [rcx]
mov rax, [r8+rdx]
ret

_VN5ArrayIhE4initEx_rPh:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov rcx, rbx
mov rsi, rax
call _V8allocatex_rPh
mov qword [rsi], rax
mov qword [rsi+8], rbx
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_VN5ArrayIhE3setExh:
mov rax, [rcx]
mov byte [rax+rdx], r8b
ret