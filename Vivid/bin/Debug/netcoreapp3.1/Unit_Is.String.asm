.section .text
.intel_syntax noprefix
.global _VN6String6appendEh_rPS_
_VN6String6appendEh_rPS_:
push rbx
push rsi
push rdi
sub rsp, 32
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
call _V4copyPhxS_
mov byte ptr [rbx+rdi], sil
add rdi, 1
mov byte ptr [rbx+rdi], 0
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _VN6String4dataEv_rPh
_VN6String4dataEv_rPh:
mov rax, [rcx+8]
ret

.global _VN6String6lengthEv_rx
_VN6String6lengthEv_rx:
xor rax, rax
mov r8, [rcx+8]
movsx rdx, byte ptr [r8+rax]
test rdx, rdx
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov r8, [rcx+8]
movsx rdx, byte ptr [r8+rax]
test rdx, rdx
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

.global _VN6String6equalsEPh_rb
_VN6String6equalsEPh_rb:
push rbx
push rsi
push rdi
sub rsp, 32
mov rbx, rcx
mov rsi, rdx
call _VN6String6lengthEv_rx
mov rcx, rsi
mov rdi, rax
call _V9length_ofPh_rx
cmp rdi, rax
je _VN6String6equalsEPh_rb_L0
xor rax, rax
add rsp, 32
pop rdi
pop rsi
pop rbx
ret
_VN6String6equalsEPh_rb_L0:
xor rcx, rcx
cmp rcx, rdi
jge _VN6String6equalsEPh_rb_L3
_VN6String6equalsEPh_rb_L2:
mov r8, [rbx+8]
movsx rdx, byte ptr [r8+rcx]
movsx r9, byte ptr [rsi+rcx]
cmp rdx, r9
je _VN6String6equalsEPh_rb_L5
xor rax, rax
add rsp, 32
pop rdi
pop rsi
pop rbx
ret
_VN6String6equalsEPh_rb_L5:
add rcx, 1
cmp rcx, rdi
jl _VN6String6equalsEPh_rb_L2
_VN6String6equalsEPh_rb_L3:
mov rax, 1
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _VN6String4initEPh_rPS_
_VN6String4initEPh_rPS_:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, 16
call _V8allocatex_rPh
mov qword ptr [rax+8], rbx
add rsp, 32
pop rbx
ret

.global _V9length_ofPh_rx
_V9length_ofPh_rx:
xor rax, rax
_V9length_ofPh_rx_L1:
_V9length_ofPh_rx_L0:
movsx rdx, byte ptr [rcx+rax]
test rdx, rdx
jne _V9length_ofPh_rx_L3
ret
_V9length_ofPh_rx_L3:
add rax, 1
jmp _V9length_ofPh_rx_L0
_V9length_ofPh_rx_L2:
ret

.section .data

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

