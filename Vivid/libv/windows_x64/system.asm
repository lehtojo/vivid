.extern GetSystemTimeAsFileTime

.global _V4timev_rx
_V4timev_rx:
lea rcx, [rsp+8] # Use the allocated space for parameters even though there are no parameters
_V4timev_rx_L0:
call GetSystemTimeAsFileTime
test rax, rax
jz _V4timev_rx_L0
mov rax, [rsp+8]
ret
