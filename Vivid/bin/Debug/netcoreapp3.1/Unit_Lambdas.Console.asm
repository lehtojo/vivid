.section .text
.intel_syntax noprefix
.global _V5printP6String
_V5printP6String:
push rbx
sub rsp, 32
mov rbx, rcx
call _VN6String4dataEv_rPh
mov rcx, rbx
mov rbx, rax
call _VN6String6lengthEv_rx
mov rcx, rbx
mov rdx, rax
call _V14internal_printPhx
add rsp, 32
pop rbx
ret

.global _V7printlnP6String
_V7printlnP6String:
push rbx
sub rsp, 32
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
add rsp, 32
pop rbx
ret

.global _V7printlnPh
_V7printlnPh:
sub rsp, 40
call _VN6String4initEPh_rPS_
mov rcx, rax
mov rdx, 10
call _VN6String6appendEh_rPS_
mov rcx, rax
call _V5printP6String
add rsp, 40
ret

.global _V7printlnx
_V7printlnx:
sub rsp, 40
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
ret

.global _V7printlnd
_V7printlnd:
sub rsp, 40
call _V9to_stringd_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
ret

.section .data

