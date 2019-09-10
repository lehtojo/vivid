section .text


; a:
; mov ecx, 0xFFFFFFFF
; mov al, 0
; mov edi, [esp+4]
; repne scasb
; mov eax, 0xFFFFFFFE
; sub eax, ecx
; ret

global a:function
a:
  mov edx, [esp + 4]
  xor eax, eax
  middle:
    inc edx 
    cmp byte [edx], 0
    inc eax
    jnz middle
ret

global b:function
b:
mov edx, [esp + 4]
xor eax, eax
jmp check
top:
inc edx
inc eax
check:
cmp byte [edx], 0
jnz top
ret