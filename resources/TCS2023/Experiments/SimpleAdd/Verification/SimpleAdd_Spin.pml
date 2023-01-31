#define MAX_STEPS 20
#define A_SIZE 1
#define C_SIZE 4
#define L_SIZE 3

int a_ = 0;
c_code {
	int a_ = 0;
};

mtype = {initial, running, rules_applied, step_complete, finished}

typedef Compartment {
	int type;
	int x[A_SIZE] = 0;
	int xt[A_SIZE] = 0;
	int links[L_SIZE] = -1;
	int lTCount[4] = 0;
	int lCount = 0;
}

Compartment c[C_SIZE];
mtype state;
int cCount = 0;
int cTCount[4] = 0;
int step = -1;
int rulesAppliedThisStep = 0;
bit deadlock = 0;

/* LTL Properties */
ltl prop1 { <> ((c[3].x[a_] > 0 -> X (state != step_complete U (c[3].x[a_] == 0 && state == step_complete))) && state == step_complete) }
ltl prop2 { [] (((c[0].x[a_] == 0 && (c[3].x[a_] == 0 && c[1].x[a_] == ((1 + 2) + 2))) -> X (state != step_complete U ((c[0].x[a_] == 0 && (c[3].x[a_] == 0 && (c[1].x[a_] == ((1 + 2) + 1) && c[2].x[a_] == 1))) && state == step_complete))) || state != step_complete) }
ltl prop3 { [] (((c[0].x[a_] > 0 && c[3].x[a_] > 0) -> <> ((c[0].x[a_] == 0 && (c[3].x[a_] == 0 && c[2].x[a_] == 1)) && state == step_complete)) || state != step_complete) }

c_code {
	int selectTarget(int ntar, int ci, int ti) {
		int cskip = -1;
		int k = -1;
		int link = -1;
		while(cskip < ntar) {
			link = now.c[ci].links[++k];
			if(link > -1 && now.c[link].type == ti) {
				++cskip;
			}
		}
		return link;
	}

	int getApplicabilityRate(int ci, int ruleLhs[], int lhsSize) {
		int i;
		int app = -1;
		for(i = 0; i < lhsSize; i += 2) {
			int obj = ruleLhs[i];
			int mult = ruleLhs[i + 1];
			if(now.c[ci].x[obj] < mult) {
				return 0;
			} else {
				int cap = now.c[ci].x[obj] / mult;
				if(app == -1) {
					app = cap;
				} else if(cap < app) {
					app = cap;
				}
			}
		}
		return app;
	}

};

active proctype KP() {
	atomic {
		c[0].type = 0;
		c[0].x[a_] = 2;
		cTCount[0] = 1;
		c[1].type = 1;
		cTCount[1] = 1;
		c[2].type = 2;
		c[2].x[a_] = 3;
		cTCount[2] = 1;
		c[3].type = 3;
		cTCount[3] = 1;
		cCount = 4;
		c[0].links[0] = 1;
		c[0].lCount = 1;
		c[0].lTCount[1] = 1;
		c[1].links[0] = 0;
		c[1].links[1] = 2;
		c[1].links[2] = 3;
		c[1].lCount = 3;
		c[1].lTCount[0] = 1;
		c[1].lTCount[2] = 1;
		c[1].lTCount[3] = 1;
		c[2].links[0] = 1;
		c[2].lCount = 1;
		c[2].lTCount[1] = 1;
		c[3].links[0] = 1;
		c[3].lCount = 1;
		c[3].lTCount[1] = 1;

		step = 0;
	}
	state = initial;
	assert(cCount > 0);
	int i;
	int ntar = 0;

	do :: step < MAX_STEPS && !deadlock ->
		atomic {
		state = running;
		rulesAppliedThisStep = 0;
		int cCountA = cCount - 1;
		for(i: 0 .. cCountA) {
			if
				/* TYPE In1 */
				:: c[i].type == 0 ->
					bit g00 = c[i].x[a_] == 1;
					bit g01 = c[i].x[a_] >= 2;
					if
						:: g00 &&
							c[i].x[a_] >= 1 &&
							c[i].lTCount[1] > 0
							 ->
							c[i].x[a_] = c[i].x[a_] - 1; 
							select(ntar: 0 .. c[i].lTCount[1] - 1);
							c_code {
								int j;
								j = selectTarget(PKP->ntar, PKP->i, 1);
								now.c[j].xt[a_] += 1;
							};
							rulesAppliedThisStep++;
						:: g01 &&
							c[i].x[a_] >= 2 &&
							c[i].lTCount[1] > 0
							 ->
							c[i].x[a_] = c[i].x[a_] - 2; 
							select(ntar: 0 .. c[i].lTCount[1] - 1);
							c_code {
								int j;
								j = selectTarget(PKP->ntar, PKP->i, 1);
								now.c[j].xt[a_] += 2;
							};
							rulesAppliedThisStep++;
						:: else -> skip;
					fi;

				/* TYPE Add */
				:: c[i].type == 1 ->
					bit g12 = c[i].x[a_] == 5;
					if
						:: g12 &&
							c[i].x[a_] >= 1 &&
							c[i].lTCount[3] > 0
							 ->
							c[i].x[a_] = c[i].x[a_] - 1; 
							select(ntar: 0 .. c[i].lTCount[3] - 1);
							c_code {
								int j;
								j = selectTarget(PKP->ntar, PKP->i, 3);
								now.c[j].xt[a_] += 1;
							};
							rulesAppliedThisStep++;
						:: else -> skip;
					fi;

				/* TYPE In2 */
				:: c[i].type == 2 ->
					bit g23 = c[i].x[a_] == 1;
					bit g24 = c[i].x[a_] >= 2;
					if
						:: g23 &&
							c[i].x[a_] >= 1 &&
							c[i].lTCount[1] > 0
							 ->
							c[i].x[a_] = c[i].x[a_] - 1; 
							select(ntar: 0 .. c[i].lTCount[1] - 1);
							c_code {
								int j;
								j = selectTarget(PKP->ntar, PKP->i, 1);
								now.c[j].xt[a_] += 1;
							};
							rulesAppliedThisStep++;
						:: g24 &&
							c[i].x[a_] >= 2 &&
							c[i].lTCount[1] > 0
							 ->
							c[i].x[a_] = c[i].x[a_] - 2; 
							select(ntar: 0 .. c[i].lTCount[1] - 1);
							c_code {
								int j;
								j = selectTarget(PKP->ntar, PKP->i, 1);
								now.c[j].xt[a_] += 2;
							};
							rulesAppliedThisStep++;
						:: else -> skip;
					fi;

				:: else -> skip;
			fi;
			ExEnd: skip;
		}

		state = rules_applied;

		c_code {
			int i, j;
			int cCount = now.cCount;
			for(i = 0; i < cCount; i++) {
				for(j = 0; j < A_SIZE; j++) {
					now.c[i].x[j] += now.c[i].xt[j];
					now.c[i].xt[j] = 0;
				}
			}
			if(now.rulesAppliedThisStep == 0) {
				now.deadlock = 1;
			} else {
				now.step++;
			}
		};
		state = step_complete;
	}
	:: else -> break;
	od;
	state = finished;
}
