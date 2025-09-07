### Threshold-Based Counter in KPL

This Kernel P System implements a **threshold-based counter**.  
Objects `b$i$` circulate between two compartment types, producing `c` objects proportional to their index on each cycle.  
When the accumulated number of `c` objects reaches a predefined constant (`THRESHOLD`), all `b$i$` objects are consumed, leaving only `c` objects in the system and causing the computation to halt.