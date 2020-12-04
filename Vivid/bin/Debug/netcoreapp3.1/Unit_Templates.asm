.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V4copyPhxS_
.extern _V11offset_copyPhxS_x
.extern _V17internal_allocatex_rPh

.global _V11create_packv_rP4PackIP7ProductP5PriceE
_V11create_packv_rP4PackIP7ProductP5PriceE:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE4initEv_rS3_
add rsp, 40
ret

.global _V11set_productP4PackIP7ProductP5PriceExPhxc
_V11set_productP4PackIP7ProductP5PriceExPhxc:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
mov rdi, r8
mov rbp, r9
call _VN7Product4initEv_rPS_
mov rcx, rdi
mov rdi, rax
call _VN6String4initEPh_rPS_
mov qword ptr [rdi+8], rax
call _VN5Price4initEv_rPS_
mov qword ptr [rax+8], rbp
mov rbp, [rsp+112]
mov byte ptr [rax+16], bpl
mov rcx, rdi
mov rdx, rax
call _VN4PairIP7ProductP5PriceE4initES0_S2__rS3_
mov rcx, rbx
mov rdx, rsi
mov r8, rax
call _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String
_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
mov rax, [rcx+8]
add rsp, 40
ret

.global _V15enchant_productP4PackIP7ProductP5PriceEx
_V15enchant_productP4PackIP7ProductP5PriceEx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
call _VN7Product7enchantEv
add rsp, 40
ret

.global _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx
_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx:
sub rsp, 40
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+8]
call _VN7Product12is_enchantedEv_rx
add rsp, 40
ret

.global _V17get_product_priceP4PackIP7ProductP5PriceExc_rd
_V17get_product_priceP4PackIP7ProductP5PriceExc_rd:
push rbx
sub rsp, 32
mov rbx, r8
call _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E
mov rcx, [rax+16]
mov rdx, rbx
call _VN5Price7convertEc_rd
add rsp, 32
pop rbx
ret

_V4initv_rx:
mov rax, 1
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

_VN7Product4initEv_rPS_:
sub rsp, 40
mov rcx, 16
call _V8allocatex_rPh
add rsp, 40
ret

_VN7Product7enchantEv:
push rbx
sub rsp, 32
mov rbx, rcx
lea rcx, [rip+_VN7Product7enchantEv_S0]
call _VN6String4initEPh_rPS_
mov rdx, [rbx+8]
mov rcx, rax
call _VN6String4plusEPS__rS0_
mov qword ptr [rbx+8], rax
add rsp, 32
pop rbx
ret

_VN7Product12is_enchantedEv_rx:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, [rbx+8]
xor rdx, rdx
call _VN6String3getEx_rh
movzx rax, al
cmp rax, 105
jne _VN7Product12is_enchantedEv_rx_L0
mov rax, 1
add rsp, 32
pop rbx
ret
_VN7Product12is_enchantedEv_rx_L0:
xor rax, rax
add rsp, 32
pop rbx
ret

_VN5Price4initEv_rPS_:
sub rsp, 40
mov rcx, 17
call _V8allocatex_rPh
add rsp, 40
ret

_VN5Price7convertEc_rd:
movsx r8, byte ptr [rcx+16]
cmp r8, rdx
jne _VN5Price7convertEc_rd_L0
mov r8, [rcx+8]
cvtsi2sd xmm0, r8
ret
_VN5Price7convertEc_rd_L0:
test rdx, rdx
jne _VN5Price7convertEc_rd_L3
mov rdx, [rcx+8]
cvtsi2sd xmm0, rdx
movsd xmm1, qword ptr [rip+_VN5Price7convertEc_rd_C0]
mulsd xmm0, xmm1
ret
jmp _VN5Price7convertEc_rd_L2
_VN5Price7convertEc_rd_L3:
mov rdx, [rcx+8]
cvtsi2sd xmm0, rdx
movsd xmm1, qword ptr [rip+_VN5Price7convertEc_rd_C1]
mulsd xmm0, xmm1
ret
_VN5Price7convertEc_rd_L2:
ret

_VN4PackIP7ProductP5PriceE4initEv_rS3_:
sub rsp, 40
mov rcx, 32
call _V8allocatex_rPh
add rsp, 40
ret

_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E:
test rdx, rdx
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L1
mov rax, [rcx+8]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L1:
cmp rdx, 1
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L3
mov rax, [rcx+16]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L3:
cmp rdx, 2
jne _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L5
mov rax, [rcx+24]
ret
jmp _VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L5:
xor rax, rax
ret
_VN4PackIP7ProductP5PriceE3getEx_rP4PairIS0_S2_E_L0:
ret

_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E:
test rdx, rdx
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L1
mov qword ptr [rcx+8], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L1:
cmp rdx, 1
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L3
mov qword ptr [rcx+16], r8
jmp _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L3:
cmp rdx, 2
jne _VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0
mov qword ptr [rcx+24], r8
_VN4PackIP7ProductP5PriceE3setExP4PairIS0_S2_E_L0:
ret

_VN4PairIP7ProductP5PriceE4initES0_S2__rS3_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
mov rsi, rdx
call _V8allocatex_rPh
mov qword ptr [rax+8], rbx
mov qword ptr [rax+16], rsi
add rsp, 40
pop rsi
pop rbx
ret

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
lea rbp, [rax+1]
lea rcx, [rdi+rbp]
call _V8allocatex_rPh
mov rcx, [rbx+8]
mov rdx, rdi
mov r8, rax
mov rbx, rax
call _V4copyPhxS_
mov rcx, [rsi+8]
mov rdx, rbp
mov r8, rbx
mov r9, rdi
call _V11offset_copyPhxS_x
mov rcx, rbx
call _VN6String4initEPh_rPS_
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

_VN6String4plusEPS__rS0_:
sub rsp, 40
call _VN6String7combineEPS__rS0_
add rsp, 40
ret

_VN6String3getEx_rh:
mov r8, [rcx+8]
movzx rax, byte ptr [r8+rdx]
ret

_VN6String6lengthEv_rx:
xor rax, rax
mov rdx, [rcx+8]
movzx r8, byte ptr [rdx+rax]
test r8, r8
je _VN6String6lengthEv_rx_L1
_VN6String6lengthEv_rx_L0:
add rax, 1
mov rdx, [rcx+8]
movzx r8, byte ptr [rdx+rax]
test r8, r8
jne _VN6String6lengthEv_rx_L0
_VN6String6lengthEv_rx_L1:
ret

.section .data

_VN10Allocation_current:
.quad 0

_VN4Pair_configuration:
.quad _VN4Pair_descriptor

_VN4Pair_descriptor:
.quad _VN4Pair_descriptor_0
.long 8
.long 0

_VN4Pair_descriptor_0:
.ascii "Pair"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4Pack_configuration:
.quad _VN4Pack_descriptor

_VN4Pack_descriptor:
.quad _VN4Pack_descriptor_0
.long 8
.long 0

_VN4Pack_descriptor_0:
.ascii "Pack"
.byte 0
.byte 1
.byte 2
.byte 0

_VN7Product_configuration:
.quad _VN7Product_descriptor

_VN7Product_descriptor:
.quad _VN7Product_descriptor_0
.long 16
.long 0

_VN7Product_descriptor_0:
.ascii "Product"
.byte 0
.byte 1
.byte 2
.byte 0

_VN5Price_configuration:
.quad _VN5Price_descriptor

_VN5Price_descriptor:
.quad _VN5Price_descriptor_0
.long 17
.long 0

_VN5Price_descriptor_0:
.ascii "Price"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4PackIP7ProductP5PriceE_configuration:
.quad _VN4PackIP7ProductP5PriceE_descriptor

_VN4PackIP7ProductP5PriceE_descriptor:
.quad _VN4PackIP7ProductP5PriceE_descriptor_0
.long 32
.long 0

_VN4PackIP7ProductP5PriceE_descriptor_0:
.ascii "Pack<Product, Price>"
.byte 0
.byte 1
.byte 2
.byte 0

_VN4PairIP7ProductP5PriceE_configuration:
.quad _VN4PairIP7ProductP5PriceE_descriptor

_VN4PairIP7ProductP5PriceE_descriptor:
.quad _VN4PairIP7ProductP5PriceE_descriptor_0
.long 24
.long 0

_VN4PairIP7ProductP5PriceE_descriptor_0:
.ascii "Pair<Product, Price>"
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

.balign 16
_VN7Product7enchantEv_S0:
.ascii "i"
.byte 0
.balign 16
_VN5Price7convertEc_rd_C0:
.byte 154, 153, 153, 153, 153, 153, 233, 63 # 0.8
.balign 16
_VN5Price7convertEc_rd_C1:
.byte 0, 0, 0, 0, 0, 0, 244, 63 # 1.25