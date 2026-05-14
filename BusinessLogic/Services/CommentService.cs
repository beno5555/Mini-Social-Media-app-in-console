using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Responses;
using social_media_console_app.Repositories;

namespace social_media_console_app.BusinessLogic.Services;

public class CommentService
{
    private readonly CommentRepository _commentRepository;
    private readonly PostRepository    _postRepository;
    private readonly CommentMapper     _commentMapper;

    public CommentService(CommentRepository commentRepository, PostRepository postRepository, CommentMapper commentMapper)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _commentMapper = commentMapper;
    }

    public async Task<Response> AddCommentAsync(CreateCommentDto createCommentDto)
    {
        var response   = new Response();
        var postExists = await _postRepository.ExistsByIdAsync(createCommentDto.PostId);

        if (postExists)
        {
            var comment = _commentMapper.ToEntity(createCommentDto);
            await _commentRepository.AddAsync(comment);
        }
        else
        {
            response.Fail("Post not found");
        }

        return response;
    }

    public async Task<Response> DeleteCommentAsync(int commentId)
    {
        var response = new Response();

        var comment = await _commentRepository.GetByIdAsync(commentId);

        if (comment is not null)
        {
            await _commentRepository.DeleteAsync(comment);  
        }
        else
        {
            response.Fail("Comment not found");
        }

        return response;
    }

    public async Task<Response<List<DisplayCommentDto>>> GetByPostAsync(int postId, int? pageNumber, int? pageSize)
    {
        var response = new Response<List<DisplayCommentDto>>();

        var postExists = await _postRepository.ExistsByIdAsync(postId);

        if (postExists)
        {
            var comments    = await _commentRepository.GetByPostIdAsync(postId, pageNumber, pageSize);
            var commentDtos = comments.Select(_commentMapper.ToDisplay).ToList();
            response.Ok(commentDtos);
        }
        else
        {
            response.Fail("Post not found");
        }

        return response;
    }
}