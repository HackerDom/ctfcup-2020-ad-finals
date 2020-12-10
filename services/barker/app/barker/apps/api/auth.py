from functools import wraps
from main.models import Token
from django.http import HttpResponse


def auth_required(func):
    @wraps(func)
    def _wrapped_func(request, *args, **kwargs):
        token = request.headers.get("Token")
        token_obj = Token.objects.select_related().filter(value=token).first()
        if not token_obj:
            return HttpResponse(status=401)
        request.user = token_obj.owner
        request.token = token_obj
        return func(request, *args, **kwargs)
    return _wrapped_func
