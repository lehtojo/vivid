section .text
global _start
_start:
call _V4initv
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V4copyPhxPS_
extern _V14internal_printPhx
extern _V4exitx

_V4initv:
push rbx
sub rsp, 16
mov rdi, 10
call _VN5ArrayIxE4initEx_rPh
mov rdi, 7
mov rbx, rax
call _VN5ArrayIdE4initEx_rPh
mov rdi, rbx
mov rsi, 7
mov rdx, 10
mov rbx, rax
call _VN5ArrayIxE3setExx
mov rdi, rbx
mov rsi, 10
movsd xmm0, qword [rel _V4initv_C0]
call _VN5ArrayIdE3setExd
add rsp, 16
pop rbx
ret

_V7requireb:
push rbx
sub rsp, 16
movzx rdi, dil
mov rax, rdi
not rax
test rax, rax
je _V7requireb_L0
movzx rax, dil
lea rdi, [rel _V7requireb_S0]
mov rbx, rax
call _V7printlnPh
mov rdi, 1
call _V4exitx
mov dil, bl
_V7requireb_L0:
add rsp, 16
pop rbx
ret

_V7requirebPh:
push rbx
push rbp
sub rsp, 8
movzx rdi, dil
mov rax, rdi
not rax
test rax, rax
je _V7requirebPh_L0
movzx rax, dil
mov rdi, rsi
mov rbx, rax
mov rbp, rsi
call _V7printlnPh
mov rdi, 1
call _V4exitx
mov dil, bl
mov rsi, rbp
_V7requirebPh_L0:
add rsp, 8
pop rbp
pop rbx
ret

_V6printsP6String:
push rbx
sub rsp, 16
mov rbx, rdi
call _VN6String4dataEv_rPh
mov rdi, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
mov rdi, rbx
mov rsi, rax
call _V14internal_printPhx
add rsp, 16
pop rbx
ret

_V7printlnPh:
sub rsp, 8
call _VN6String4initEPh_rS0_
mov rdi, rax
mov rsi, 10
call _VN6String6appendEh_rPS_
mov rdi, rax
call _V6printsP6String
add rsp, 8
ret

_VN6String4initEPh_rS0_:
push rbx
sub rsp, 16
mov rcx, rdi
mov rdi, 8
mov rbx, rcx
call _V8allocatex_rPh
mov qword [rax], rbx
add rsp, 16
pop rbx
ret

_VN6String6appendEh_rPS_:
push rbx
push rbp
push r12
sub rsp, 16
mov rbx, rsi
mov rbp, rdi
call _VN6String6lengthEv_rx
lea rdi, [rax+2]
mov r12, rax
call _V8allocatex_rPh
mov rdi, [rbp]
mov rsi, r12
mov rdx, rax
mov rbp, rax
call _V4copyPhxPS_
mov byte [rbp+r12], bl
add r12, 1
mov byte [rbp+r12], 0
mov rdi, rbp
call _VN6String4initEPh_rS0_
add rsp, 16
pop r12
pop rbp
pop rbx
ret

_VN6String4dataEv_rPh:
mov rax, [rdi]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov rdx, [rdi]
movzx rcx, byte [rdx+rax]
test rcx, rcx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov rdx, [rdi]
movzx rcx, byte [rdx+rax]
test rcx, rcx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

_VN5ArrayIxE4initEx_rPh:
push rbx
push rbp
sub rsp, 8
mov rcx, rdi
mov rdi, 16
mov rbx, rcx
call _V8allocatex_rPh
xor dil, dil
test rbx, rbx
jl _VN5ArrayIxE4initEx_rPh_L0
mov dil, 1
_VN5ArrayIxE4initEx_rPh_L0:
movzx rcx, dil
movzx rdi, cl
lea rsi, [rel _VN5ArrayIxE4initEx_rPh_S0]
mov rbp, rax
call _V7requirebPh
mov rdi, rbx
sal rdi, 3
mov rcx, rdi
mov rdi, rcx
call _V8allocatex_rPh
mov qword [rbp], rax
mov qword [rbp+8], rbx
mov rax, rbp
add rsp, 8
pop rbp
pop rbx
ret

_VN5ArrayIxE3setExx:
push rbx
push rbp
push r12
sub rsp, 16
xor al, al
test rsi, rsi
jl _VN5ArrayIxE3setExx_L0
cmp rsi, [rdi+8]
jge _VN5ArrayIxE3setExx_L0
mov al, 1
_VN5ArrayIxE3setExx_L0:
mov rcx, rdi
movzx rdi, al
mov rbx, rcx
mov rbp, rdx
mov r12, rsi
call _V7requireb
sal r12, 3
mov rax, [rbx]
mov qword [rax+r12], rbp
add rsp, 16
pop r12
pop rbp
pop rbx
ret

_VN5ArrayIdE4initEx_rPh:
push rbx
push rbp
sub rsp, 8
mov rcx, rdi
mov rdi, 16
mov rbx, rcx
call _V8allocatex_rPh
xor dil, dil
test rbx, rbx
jl _VN5ArrayIdE4initEx_rPh_L0
mov dil, 1
_VN5ArrayIdE4initEx_rPh_L0:
movzx rcx, dil
movzx rdi, cl
lea rsi, [rel _VN5ArrayIdE4initEx_rPh_S0]
mov rbp, rax
call _V7requirebPh
mov rdi, rbx
sal rdi, 3
mov rcx, rdi
mov rdi, rcx
call _V8allocatex_rPh
mov qword [rbp], rax
mov qword [rbp+8], rbx
mov rax, rbp
add rsp, 8
pop rbp
pop rbx
ret

_VN5ArrayIdE3setExd:
push rbx
push rbp
sub rsp, 24
xor al, al
test rsi, rsi
jl _VN5ArrayIdE3setExd_L0
cmp rsi, [rdi+8]
jge _VN5ArrayIdE3setExd_L0
mov al, 1
_VN5ArrayIdE3setExd_L0:
mov rcx, rdi
movzx rdi, al
mov rbx, rcx
mov rbp, rsi
movsd qword [rsp+16], xmm0
call _V7requireb
sal rbp, 3
mov rax, [rbx]
movsd xmm0, qword [rsp+16]
movsd qword [rax+rbp], xmm0
add rsp, 24
pop rbp
pop rbx
ret

section .data

_V7requireb_S0 db 'Requirement failed', 0
_VN5ArrayIxE4initEx_rPh_S0 db 'Tried to create a standard array but its size was a negative value', 0
_VN5ArrayIdE4initEx_rPh_S0 db 'Tried to create a standard array but its size was a negative value', 0
_V4initv_C0 dq 7.0