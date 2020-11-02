section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V4copyPhxPS_
extern _V11offset_copyPhxPS_x
extern _V14internal_printPhx

_V9fibonaccix:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 48
xor rax, rax
xor rdx, rdx
mov r8, 1
xor r9, r9
cmp rax, rcx
jge _V9fibonaccix_L1
_V9fibonaccix_L0:
cmp rax, 1
jg _V9fibonaccix_L4
mov rdx, rax
jmp _V9fibonaccix_L3
_V9fibonaccix_L4:
lea rdx, [r9+r8]
mov r9, r8
mov r8, rdx
_V9fibonaccix_L3:
mov rbx, rcx
mov rcx, rdx
mov rsi, rax
mov rdi, rdx
mov rbp, r8
mov r12, r9
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rsi, 1
mov rax, rsi
mov rcx, rbx
mov rdx, rdi
mov r9, r12
mov r8, rbp
cmp rax, rcx
jl _V9fibonaccix_L0
_V9fibonaccix_L1:
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V4initv_rx:
sub rsp, 40
mov rcx, 10
call _V9fibonaccix
xor rax, rax
add rsp, 40
ret

_V9to_stringx_rP6String:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
lea rcx, [rel _V9to_stringx_rP6String_S0]
call _VN6String4initEPh_rPS_
lea rcx, [rel _V9to_stringx_rP6String_S1]
mov rsi, rax
call _VN6String4initEPh_rPS_
test rbx, rbx
jge _V9to_stringx_rP6String_L0
lea rcx, [rel _V9to_stringx_rP6String_S2]
call _VN6String4initEPh_rPS_
neg rbx
_V9to_stringx_rP6String_L0:
_V9to_stringx_rP6String_L3:
_V9to_stringx_rP6String_L2:
mov rdi, rax
mov rax, rbx
cqo
mov rcx, 10
idiv rcx
mov rax, rbx
mov rbp, rdx
mov rcx, 1844674407370955162
mul rcx
mov rbx, rdx
sar rbx, 63
add rbx, rdx
mov r8, 48
add r8, rbp
mov rcx, rsi
xor rdx, rdx
call _VN6String6insertExh_rPS_
test rbx, rbx
jne _V9to_stringx_rP6String_L5
mov rcx, rdi
mov rdx, rax
mov rsi, rax
call _VN6String7combineEPS__rS0_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret
mov rax, rsi
_V9to_stringx_rP6String_L5:
mov rsi, rax
mov rax, rdi
jmp _V9to_stringx_rP6String_L2
_V9to_stringx_rP6String_L4:
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V7printlnP6String:
push rbx
sub rsp, 48
mov rdx, 10
mov rbx, rcx
call _VN6String6appendEh_rPS_
mov rcx, rax
call _VN6String4dataEv_rPh
mov rcx, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
add rax, 1
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 48
pop rbx
ret

_VN6String4initEPh_rPS_:
push rbx
sub rsp, 48
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov qword [rax+8], rbx
add rsp, 48
pop rbx
ret

_VN6String7combineEPS__rS0_:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
mov rcx, rsi
mov rdi, rax
call _VN6String6lengthEv_rx
add rax, 1
lea rcx, [rdi+rax]
mov rbp, rax
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxPS_
mov rcx, [rsi+8]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call _V11offset_copyPhxPS_x
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_VN6String6appendEh_rPS_:
push rbx
push rsi
push rdi
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
lea rcx, [rax+2]
mov rdi, rax
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxPS_
mov byte [rbx+rdi], sil
add rdi, 1
mov byte [rbx+rdi], 0
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 48
pop rdi
pop rsi
pop rbx
ret

_VN6String6insertExh_rPS_:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
mov rdi, r8
call _VN6String6lengthEv_rx
lea rcx, [rax+2]
mov rbp, rax
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rsi
mov r8, rax
mov r12, rax
call _V4copyPhxPS_
mov rcx, rbp
sub rcx, rsi
lea r9, [rsi+1]
mov rdx, rcx
mov rcx, [rbx+8]
mov r8, r12
call _V11offset_copyPhxPS_x
mov byte [r12+rsi], dil
add rbp, 1
mov byte [r12+rbp], 0
mov rcx, r12
call _VN6String4initEPh_rPS_
add rsp, 48
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

_VN6String4dataEv_rPh:
mov rax, [rcx+8]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov r8, [rcx+8]
movzx rdx, byte [r8+rax]
test rdx, rdx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov r8, [rcx+8]
movzx rdx, byte [r8+rax]
test rdx, rdx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

section .data

_VN6String_configuration:
dq _VN6String_descriptor

_VN6String_descriptor:
dq _VN6String_descriptor_0
dd 16
dd 0

_VN6String_descriptor_0:
db 'String', 0

align 16
_V9to_stringx_rP6String_S0 db 0
align 16
_V9to_stringx_rP6String_S1 db 0
align 16
_V9to_stringx_rP6String_S2 db '-', 0