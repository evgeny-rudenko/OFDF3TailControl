select 
date_cheque,
summ,
kkm_cheque_number,
fiscal_sign,
fiscal_document,
hash = ltrim( str (fiscal_document))+'|'+ ltrim (str(FISCAL_SIGN))
from cheque
where date_cheque between @MINDATE and @MAXDATE
order by date_cheque