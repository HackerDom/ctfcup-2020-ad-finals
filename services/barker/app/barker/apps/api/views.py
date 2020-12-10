from django.http import JsonResponse, Http404, HttpResponse
from django.views.decorators.http import require_GET

from .auth import auth_required
from main.models import Bark, Comment
from main.utils import is_friends
from django.contrib.auth.models import User


@require_GET
@auth_required
def index(request):
    user_info = {
        "id": request.user.id,
        "username": request.user.username,
        "first_name": request.user.first_name,
        "last_name": request.user.last_name,
        "token": request.token.value
    }
    return JsonResponse(user_info, safe=False)


@require_GET
@auth_required
def get_barks(request, username):
    user = User.objects.filter(username=username).first()
    if not user:
        return Http404

    barks_list = []
    barks = Bark.objects.filter(user=user)
    for bark in barks:
        if not bark.is_private or bark.is_private and is_friends(user, request.user):
            barks_list.append({
                "id": bark.id,
                "text": bark.text,
                "publication_date": bark.pub_date,
                "is_private": bark.is_private
            })
    return JsonResponse(barks_list, safe=False)


@require_GET
@auth_required
def get_last_barks(request, page_n):
    if page_n < 0:
        page_n = 0
    offset = 20
    last_barks = Bark.objects.select_related().order_by("-pub_date")[page_n * offset:(page_n + 1) * offset]
    last_barks_list = []
    for bark in last_barks:
        if not bark.is_private or bark.is_private and is_friends(bark.user, request.user):
            last_barks_list.append({
                "id": bark.id,
                "text": bark.text,
                "publication_date": bark.pub_date,
                "is_private": bark.is_private
            })
    return JsonResponse(last_barks_list, safe=False)


@require_GET
@auth_required
def get_users(request, page_n):
    if page_n < 0:
        page_n = 0
    offset = 20
    users = User.objects.order_by("username")[page_n * offset:(page_n + 1) * offset]
    user_list = []
    for user in users:
        user_list.append({
            "id": user.id,
            "username": user.username,
            "first_name": user.first_name,
            "last_name": user.last_name
        })
    return JsonResponse(user_list, safe=False)


@require_GET
@auth_required
def get_comments(request, bark_id):
    bark = Bark.objects.select_related().filter(id=bark_id).first()
    if not bark:
        return Http404

    if bark.is_private and not is_friends(bark.user, request.user):
        return HttpResponse(status=403)

    comments_list = []
    comments = Comment.objects.select_related().filter(bark=bark)
    for comment in comments:
        comments_list.append({
            "id": comment.id,
            "user": {
                "id": comment.user.id,
                "username": comment.user.username,
                "first_name": comment.user.first_name,
                "last_name": comment.user.last_name
            },
            "text": comment.text,
            "publication_date": comment.pub_date,
            "is_private": comment.is_private
        })
    return JsonResponse(comments_list, safe=False)


@require_GET
@auth_required
def get_user(request, username):
    user = User.objects.filter(username=username).first()
    if not user:
        return Http404

    user_info = {
        "id": user.id,
        "username": user.username,
        "first_name": user.first_name,
        "last_name": user.last_name
    }
    return JsonResponse(user_info, safe=False)
