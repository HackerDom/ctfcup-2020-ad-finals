import hashlib

from django.http import HttpResponse, HttpResponseRedirect, JsonResponse
from django.shortcuts import render
from django.contrib.auth.models import User
from django.contrib.auth.decorators import login_required
from django.contrib import auth
from django.views.decorators.http import require_http_methods, require_GET, require_POST

from .models import Bark, Comment, Friendship, Token
from .utils import get_user_friends_info, is_friends
from .apps import MainConfig


@require_GET
def index(request):
    if request.user.is_authenticated:
        return HttpResponseRedirect(f"/{request.user.username}/")
    return render(request, 'main/index.html')


@require_http_methods(["GET", "POST"])
def login(request):
    if request.method == "GET":
        return render(request, 'main/login.html')

    user = auth.authenticate(username=request.POST['username'], password=request.POST['password'])
    if user is not None and user.is_active:
        auth.login(request, user)
        return HttpResponseRedirect(f"/{user.username}/")
    return HttpResponseRedirect('/')


@require_GET
@login_required(login_url='/')
def logout(request):
    auth.logout(request)
    return HttpResponseRedirect('/')


@require_http_methods(["GET", "POST"])
def signup(request):
    if request.method == "GET":
        return render(request, 'main/signup.html')

    new_user = User.objects.create_user(username=request.POST['username'],
                                        password=request.POST['password'],
                                        first_name=request.POST['fname'],
                                        last_name=request.POST['lname'])
    new_user.save()

    return login(request)


@require_POST
@login_required(login_url='/')
def add_bark(request):
    new_bark = Bark.objects.create(user=request.user, text=request.POST['bark_text'],
                                   is_private='is_private' in request.POST)
    return HttpResponseRedirect(f"/{request.user.username}/")


@require_GET
def get_barks(request, username):
    user = User.objects.filter(username=username).first()
    barks = Bark.objects.filter(user=user).order_by("-pub_date")
    new_barks = Bark.objects.select_related().order_by("-pub_date")[:10]
    return render(request, 'main/barks.html', {'barks': barks,
                                               'requested_user': user,
                                               'new_barks': new_barks,
                                               'is_friend': is_friends(request.user, user)
                                               if request.user.is_authenticated else False})


@require_GET
def get_bark(request, bark_id):
    bark = Bark.objects.filter(id=bark_id).first()
    if not bark:
        return HttpResponseRedirect('')

    comments = Comment.objects.select_related().filter(bark=bark).order_by("pub_date")
    return render(request, 'main/bark.html', {'bark': bark, 'comments': comments})


@require_POST
@login_required(login_url="/")
def leave_comment(request, bark_id):
    bark = Bark.objects.filter(id=bark_id).first()
    comment = Comment.objects.create(bark=bark, user=request.user, text=request.POST['comment_text'],
                                     is_private='is_private' in request.POST)
    return HttpResponseRedirect(f"/get_bark/{bark_id}")


@require_GET
@login_required(login_url="/")
def show_friends(request):
    return render(request, 'main/friends.html', {'user_friends_info': get_user_friends_info(request.user)})


@require_GET
@login_required(login_url="/")
def add_friend(request, username):
    user = User.objects.filter(username=username).first()
    if not (Friendship.objects.filter(first_user=request.user, second_user=user).first() or
            Friendship.objects.filter(first_user=user, second_user=request.user).first()):
        Friendship.objects.create(first_user=request.user, second_user=user)
    return HttpResponseRedirect("/friends/")


@require_GET
@login_required(login_url="/")
def confirm_friend(request, username):
    user = User.objects.filter(username=username).first()
    if friendship := Friendship.objects.filter(first_user=request.user, second_user=user).first() or \
                     Friendship.objects.filter(first_user=user, second_user=request.user).first():
        friendship.status = True
        friendship.save()
    return HttpResponseRedirect("/friends/")


@require_GET
@login_required(login_url="/")
def revoke_friend(request, username):
    user = User.objects.filter(username=username).first()
    Friendship.objects.filter(first_user=request.user, second_user=user).delete()
    Friendship.objects.filter(first_user=user, second_user=request.user).delete()
    return HttpResponseRedirect("/friends/")


@require_GET
@login_required(login_url="/")
def get_profile(request):
    tokens = Token.objects.filter(owner=request.user)
    return render(request, "main/profile.html", {"tokens": tokens})


@require_GET
@login_required(login_url="/")
def generate_token(request):
    value = hashlib.md5(f"{request.user.username}{str(next(MainConfig.generator))}".encode()).hexdigest()
    Token.objects.create(owner=request.user, value=value)
    return HttpResponseRedirect("/profile/")
