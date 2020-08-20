section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V14internal_printPhx

_V8pidigitsx_rPh:
push rbx
push rbp
push r12
push r13
push r14
push r15
sub rsp, 88
add rdi, 1
mov rbx, rdi
imul rbx, 10
mov rax, rbx
mov rcx, 6148914691236517206
mul rcx
mov rbx, rdx
sar rbx, 63
add rbx, rdx
add rbx, 2
mov rcx, rdi
mov rdi, rbx
mov rbp, rcx
call _VN5ArrayIxE4initEx_rPh
mov rdi, rbx
mov r12, rax
call _VN5ArrayIxE4initEx_rPh
mov rdi, rbp
mov r13, rax
call _VN5ArrayIxE4initEx_rPh
xor r14, r14
cmp r14, rbx
jge _V8pidigitsx_rPh_L1
_V8pidigitsx_rPh_L0:
mov rdi, r12
mov rsi, r14
mov rdx, 20
mov r15, rax
call _VN5ArrayIxE3setExx
add r14, 1
cmp r14, rbx
mov rax, r15
jl _V8pidigitsx_rPh_L0
_V8pidigitsx_rPh_L1:
xor r15, r15
mov qword [rsp+80], r15
mov r15, rax
mov rcx, [rsp+80]
cmp rcx, rbp
jge _V8pidigitsx_rPh_L3
_V8pidigitsx_rPh_L2:
xor r14, r14
mov qword [rsp+72], rbp
xor rbp, rbp
cmp r14, rbx
jge _V8pidigitsx_rPh_L5
_V8pidigitsx_rPh_L4:
mov rdx, rbx
sub rdx, r14
sub rdx, 1
mov rsi, rdx
sal rsi, 1
add rsi, 1
mov rdi, r12
mov r8, rsi
mov rsi, r14
mov qword [rsp+80], rcx
mov qword [rsp+64], rdx
mov qword [rsp+56], r8
call _VN5ArrayIxE3getEx_rx
add rax, rbp
mov rdi, r12
mov rsi, r14
mov rdx, rax
call _VN5ArrayIxE3setExx
mov rdi, r12
mov rsi, r14
call _VN5ArrayIxE3getEx_rx
xor rdx, rdx
mov rcx, [rsp+56]
idiv rcx
mov rdi, r12
mov rsi, r14
mov qword [rsp+48], rax
mov qword [rsp+56], rcx
call _VN5ArrayIxE3getEx_rx
xor rdx, rdx
idiv qword [rsp+56]
mov rdi, r13
mov rsi, r14
mov rcx, rdx
mov rdx, rcx
call _VN5ArrayIxE3setExx
mov rcx, [rsp+48]
imul rcx, [rsp+64]
add r14, 1
cmp r14, rbx
mov rbp, rcx
mov rcx, [rsp+80]
jl _V8pidigitsx_rPh_L4
_V8pidigitsx_rPh_L5:
mov rsi, rbx
sub rsi, 1
mov rdi, r12
mov rdx, rsi
mov rsi, rdx
mov qword [rsp+80], rcx
call _VN5ArrayIxE3getEx_rx
mov rcx, 1844674407370955162
mul rcx
mov rcx, rdx
sar rcx, 63
add rcx, rdx
mov rdi, r15
mov rsi, [rsp+80]
mov rdx, rcx
call _VN5ArrayIxE3setExx
mov rcx, rbx
sub rcx, 1
mov rsi, rbx
sub rsi, 1
mov rdi, r12
mov rdx, rsi
mov rsi, rdx
mov qword [rsp+24], rcx
call _VN5ArrayIxE3getEx_rx
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rdi, r13
mov rsi, [rsp+24]
mov rcx, rdx
mov rdx, rcx
call _VN5ArrayIxE3setExx
xor r14, r14
cmp r14, rbx
jge _V8pidigitsx_rPh_L7
_V8pidigitsx_rPh_L6:
mov rdi, r13
mov rsi, r14
call _VN5ArrayIxE3getEx_rx
imul rax, 10
mov rdi, r12
mov rsi, r14
mov rdx, rax
call _VN5ArrayIxE3setExx
add r14, 1
cmp r14, rbx
jl _V8pidigitsx_rPh_L6
_V8pidigitsx_rPh_L7:
add qword [rsp+80], 1
mov rcx, [rsp+80]
mov rbp, [rsp+72]
cmp rcx, rbp
jl _V8pidigitsx_rPh_L2
_V8pidigitsx_rPh_L3:
mov rdx, rbp
sal rdx, 3
mov rdi, rdx
mov qword [rsp+80], rcx
call _VN5ArrayIhE4initEx_rPh
mov rcx, rbp
sub rcx, 1
mov qword [rsp+40], rbx
mov rbx, rcx
mov qword [rsp+72], rbp
xor rbp, rbp
mov qword [rsp+32], r12
mov r12, rax
test rbx, rbx
jl _V8pidigitsx_rPh_L9
_V8pidigitsx_rPh_L8:
mov rdi, r15
mov rsi, rbx
call _VN5ArrayIxE3getEx_rx
add rax, rbp
mov rdi, r15
mov rsi, rbx
mov rdx, rax
call _VN5ArrayIxE3setExx
mov rdi, r15
mov rsi, rbx
call _VN5ArrayIxE3getEx_rx
mov rcx, 1844674407370955162
mul rcx
mov rbp, rdx
sar rbp, 63
add rbp, rdx
mov rdi, r15
mov rsi, rbx
call _VN5ArrayIxE3getEx_rx
xor rdx, rdx
mov rcx, 10
idiv rcx
mov rcx, 48
add rcx, rdx
mov rdi, r12
mov rsi, rbx
mov rdx, rcx
call _VN5ArrayIhE3setExh
sub rbx, 1
test rbx, rbx
jge _V8pidigitsx_rPh_L8
_V8pidigitsx_rPh_L9:
mov rax, [r12]
add rsp, 88
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rbx
ret

_V4initv_rx:
sub rsp, 8
mov rdi, 3141
call _V8pidigitsx_rPh
mov rdi, rax
call _V5printPh
xor rax, rax
add rsp, 8
ret

_V9length_ofPh_rx:
xor rax, rax
_V9length_ofPh_rx_L0:
movzx rcx, byte [rdi+rax]
test rcx, rcx
jne _V9length_ofPh_rx_L1
ret
_V9length_ofPh_rx_L1:
add rax, 1
jmp _V9length_ofPh_rx_L0
ret

_V5printPh:
push rbx
sub rsp, 16
mov rbx, rdi
call _V9length_ofPh_rx
mov rdi, rbx
mov rsi, rax
call _V14internal_printPhx
add rsp, 16
pop rbx
ret

_VN5ArrayIxE4initEx_rPh:
push rbx
push rbp
sub rsp, 8
mov rcx, rdi
mov rdi, 16
mov rbx, rcx
call _V8allocatex_rPh
mov rdi, rbx
sal rdi, 3
mov rcx, rdi
mov rdi, rcx
mov rbp, rax
call _V8allocatex_rPh
mov qword [rbp], rax
mov qword [rbp+8], rbx
mov rax, rbp
add rsp, 8
pop rbp
pop rbx
ret

_VN5ArrayIxE3setExx:
sal rsi, 3
mov rax, [rdi]
mov qword [rax+rsi], rdx
ret

_VN5ArrayIxE3getEx_rx:
sal rsi, 3
mov rcx, [rdi]
mov rax, [rcx+rsi]
ret

_VN5ArrayIhE4initEx_rPh:
push rbx
push rbp
sub rsp, 8
mov rcx, rdi
mov rdi, 16
mov rbx, rcx
call _V8allocatex_rPh
mov rdi, rbx
sal rdi, 0
mov rcx, rdi
mov rdi, rcx
mov rbp, rax
call _V8allocatex_rPh
mov qword [rbp], rax
mov qword [rbp+8], rbx
mov rax, rbp
add rsp, 8
pop rbp
pop rbx
ret

_VN5ArrayIhE3setExh:
sal rsi, 0
mov rax, [rdi]
mov byte [rax+rsi], dl
ret